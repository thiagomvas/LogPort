using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LogPort.Core;
using LogPort.Core.Models;
using LogPort.SDK.Events;
using LogPort.SDK.Filters;

namespace LogPort.SDK;

/// <summary>
/// Represents a client for sending structured logs to a LogPort server over WebSocket.
/// </summary>
/// <remarks>
/// Supports asynchronous, non-blocking logging with an internal queue. Use <see cref="EnsureConnectedAsync"/> 
/// before sending logs. Call <see cref="FlushAsync"/> to ensure all queued logs are sent before shutdown.
/// Implements <see cref="IDisposable"/> to clean up WebSocket and cancellation resources.
/// </remarks>
public sealed class LogPortClient : IDisposable, IAsyncDisposable
{
    private readonly Uri _serverUri;
    private IWebSocketClient _webSocket;
    private readonly Func<IWebSocketClient> _socketFactory;
    private readonly ConcurrentQueue<LogEntry> _messageQueue;
    private readonly CancellationTokenSource _cts;
    private Task? _senderTask;
    private readonly TimeSpan _maxReconnectDelay;
    private readonly TimeSpan _heartbeatTimeout;
    private readonly TimeSpan _heartbeatInterval;
    private readonly ILogPortLogger? _logger;
    private readonly IEnumerable<ILogLevelFilter>? _filters;
    private readonly bool _shouldReconnect;
    private readonly string _serviceName;
    public LogPortClientState State { get; private set; } = LogPortClientState.Disconnected;
    public event EventHandler<LogPortClientStateChangedEventArgs>? StateChanged;

    private readonly LogNormalizer _normalizer;

    private const int SendDelayMs = 50;
    bool _isAlive = false;

    internal Uri ServerUri => _serverUri;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogPortClient"/> class using the provided configuration.
    /// </summary>
    /// <param name="config">Client configuration containing connection and filtering settings.</param>
    /// <param name="normalizer">Optional log level normalizer.</param>
    /// <param name="socketFactory">Optional factory for creating WebSocket clients.</param>
    /// <param name="logger">Optional internal logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the agent URL is missing.</exception>
    public LogPortClient(
        LogPortClientConfig config,
        LogNormalizer? normalizer,
        Func<IWebSocketClient>? socketFactory = null,
        ILogPortLogger? logger = null)

    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentException.ThrowIfNullOrEmpty(config.AgentUrl);
        var baseUrl = config.AgentUrl.Trim('/');

        // Remove http or https and convert to ws or wss
        if (baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = "ws://" + baseUrl.Substring("http://".Length);
        }
        else if (baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = "wss://" + baseUrl.Substring("https://".Length);
        }
        else if (!baseUrl.StartsWith("ws://", StringComparison.OrdinalIgnoreCase) &&
                 !baseUrl.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = "ws://" + baseUrl;
        }
        _serverUri = new Uri($"{baseUrl}/agent/stream");
        _socketFactory = socketFactory ?? (() => new WebSocketClientAdapter(config.ApiSecret));
        _webSocket = _socketFactory();
        _messageQueue = new ConcurrentQueue<LogEntry>();
        _cts = new CancellationTokenSource();
        _filters = config.Filters;
        _maxReconnectDelay = config.ClientMaxReconnectDelay;
        _heartbeatInterval = config.ClientHeartbeatInterval;
        _heartbeatTimeout = config.ClientHeartbeatTimeout;
        _shouldReconnect = config.AutomaticReconnect;
        _serviceName = config.ServiceName;

        _normalizer = normalizer ?? new LogNormalizer();
        _logger = logger;
    }

    public LogPortClient(LogPortClientConfig config, Func<IWebSocketClient>? socketFactory = null)
        : this(config, null, socketFactory)
    {

    }

    private LogPortClient(string serverUrl, Func<IWebSocketClient>? socketFactory = null)
        : this(new LogPortClientConfig() { AgentUrl = serverUrl }, null, socketFactory)
    {
    }

    /// <summary>
    /// Creates a <see cref="LogPortClient"/> using the server URL specified in the environment variable LOGPORT_SERVER_URL.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the environment variable is not set.</exception>
    public static LogPortClient FromEnvironment()
    {
        var config = LogPortClientConfig.LoadFromEnvironment();

        return new LogPortClient(config.AgentUrl);
    }

    /// <summary>
    /// Creates a <see cref="LogPortClient"/> with a specific server URL.
    /// </summary>
    /// <param name="serverUrl">The WebSocket URL of the LogPort server (e.g., ws://localhost:8080).</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="serverUrl"/> is null or empty.</exception>
    public static LogPortClient FromServerUrl(string serverUrl)
    {
        if (string.IsNullOrWhiteSpace(serverUrl))
            throw new ArgumentException("Server URL must be provided.", nameof(serverUrl));

        return new LogPortClient(serverUrl);
    }

    /// <summary>
    /// Connects to the LogPort server and starts the background task that sends queued logs.
    /// </summary>
    /// <remarks>
    /// Must be called before sending logs. Subsequent calls when already connected have no effect.
    /// </remarks>
    public async Task EnsureConnectedAsync(CancellationToken token = default)
    {
        if (_senderTask is { IsCompleted: false })
            return; // already running

        await EnsureSocketConnectedAsync(token).ConfigureAwait(false);
        _senderTask = Task.Run(async () =>
        {
            try
            {
                await ProcessQueueAsync(_cts.Token);
            }
            catch (TaskCanceledException)
            {
            }
        }, token);
    }


    /// <summary>
    /// Enqueues a <see cref="LogEntry"/> to be sent asynchronously to the server.
    /// </summary>
    /// <param name="entry">The log entry to send.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.</exception>
    public void Log(LogEntry entry)
    {
        if (entry is null) throw new ArgumentNullException(nameof(entry));
        if (entry.Timestamp == DateTime.MinValue) entry.Timestamp = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(entry.ServiceName)) entry.ServiceName = _serviceName;

        entry.Level = _normalizer.NormalizeLevel(entry.Level);
        

        if (_filters != null && _filters.Any(filter => !filter.ShouldSend(entry)))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(entry.TraceId))
            entry.TraceId = TraceContext.TraceId;

        if (string.IsNullOrWhiteSpace(entry.SpanId))
            entry.SpanId = TraceContext.SpanId;

        _messageQueue.Enqueue(entry);
    }

    /// <summary>
    /// Enqueues a batch of <see cref="LogEntry"/> items to be sent asynchronously to the server.
    /// </summary>
    /// <param name="entries">An <see cref="IEnumerable{T}"/> containing the log batch to be enqueued.</param>
    /// <exception cref="ArgumentNullException">Thrown if any entry is null.</exception>
    public void LogBatch(IEnumerable<LogEntry> entries)
    {
        if (entries is null) throw new ArgumentNullException(nameof(entries));

        foreach (var entry in entries)
        {
            Log(entry);
        }
    }

    /// <summary>
    /// Convenience method to log a simple message with a specified level.
    /// </summary>
    /// <param name="level">The log level (e.g., "Info", "Error").</param>
    /// <param name="message">The log message.</param>
    public void Log(string level, string message)
    {
        Log(new LogEntry() { Level = level, Message = message, Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Attaches the client to the standard console output and error streams.
    /// </summary>
    /// <remarks>
    /// This redirects all console output to the LogPort server as log entries by adding a
    /// <see cref="LogPortTextWriterDecorator"/> to the console streams. Due to that, it will
    /// lose the attached behaviour if the console streams are changed afterwards. Be careful
    /// if you plan on using other libraries that might change the console output and also
    /// redirecting the Console streams to LogPort.
    /// </remarks>
    public void AttachToConsole()
    {
        Console.SetOut(new LogPortTextWriterDecorator(Console.Out, this));
        Console.SetError(new LogPortTextWriterDecorator(Console.Error, this));
    }

    private async Task ProcessQueueAsync(CancellationToken token)
    {
        var lastHeartbeat = DateTime.UtcNow;

        while (!token.IsCancellationRequested)
        {
            await EnsureSocketConnectedAsync(token);

            while (_webSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                if (_messageQueue.TryPeek(out var entry))
                {
                    try
                    {
                        string json = JsonSerializer.Serialize(entry);
                        var bytes = Encoding.UTF8.GetBytes(json);

                        await _webSocket.SendAsync(
                            new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text,
                            true,
                            token
                        ).ConfigureAwait(false);
                        _messageQueue.TryDequeue(out _);
                    }
                    catch
                    {
                        _webSocket.Abort();
                        _webSocket.Dispose();
                        break; // exit inner loop, outer loop will reconnect
                    }
                }
                else
                {
                    var now = DateTime.UtcNow;
                    if (now - lastHeartbeat >= _heartbeatInterval)
                    {
                        bool alive = await SendHeartbeatAsync(token);
                        lastHeartbeat = now;

                        if (!alive)
                        {
                            _webSocket.Abort();
                            _webSocket.Dispose();
                            break; // exit inner loop, outer loop will reconnect
                        }
                    }

                    await Task.Delay(SendDelayMs, token).ConfigureAwait(false);
                }
            }

            if (_webSocket.State != WebSocketState.Open)
            {
                // Small delay before retrying connection
                await Task.Delay(1000, token).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Asynchronously waits for all queued log messages to be sent to the server,
    /// up to a default maximum wait time.
    /// </summary>
    /// <remarks>
    /// This is a convenience overload that uses a default timeout of five seconds.
    /// If the queue is not fully flushed within that time, the method returns without throwing.
    /// </remarks>
    public Task<bool> FlushAsync() => FlushAsync(TimeSpan.FromSeconds(5));

    /// <summary>
    /// Asynchronously waits for the internal log queue to be flushed, up to a maximum amount of time.
    /// </summary>
    /// <param name="maxWait">
    /// The maximum duration to wait for all queued log messages to be sent.
    /// </param>
    /// <returns>
    /// <c>true</c> if the log queue was fully flushed before the deadline elapsed;
    /// <c>false</c> if one or more log entries were still pending when the timeout was reached.
    /// </returns>
    /// <remarks>
    /// This method polls the internal queue at a short interval and returns as soon as the queue
    /// becomes empty or the specified timeout expires. It does not throw on timeout.
    /// </remarks>
    public async Task<bool> FlushAsync(TimeSpan maxWait)
    {
        _logger?.Debug($"Flushing log queue with timeout duration of {maxWait.TotalSeconds} seconds");
        var deadline = DateTime.UtcNow + maxWait;

        while (!_messageQueue.IsEmpty && DateTime.UtcNow < deadline)
        {
            await Task.Delay(25);
        }

        return _messageQueue.IsEmpty;
    }


    private void SetState(LogPortClientState newState)
    {
        if (State == newState)
            return;

        var old = State;
        State = newState;
        StateChanged?.Invoke(this, new(old, newState));

    }

    /// <summary>
    /// Stops background processing, cancels pending sends, and releases all resources.
    /// </summary>
    /// <remarks>
    /// Should be called when the client is no longer needed. After disposal, the client cannot be reused.
    /// </remarks>
    public void Dispose()
    {
        _logger?.Debug("Disposing LogPortClient...");
        _cts.Cancel();
        _senderTask?.Wait();
        _webSocket.CloseConnection(WebSocketCloseStatus.NormalClosure, "Client disposed");
        _webSocket.Dispose();
        _cts.Dispose();
        _logger?.Debug("LogPortClient disposed.");
        SetState(LogPortClientState.Stopped);
    }

    private async Task<bool> SendHeartbeatAsync(CancellationToken token)
    {
        try
        {
            var pingBytes = Encoding.UTF8.GetBytes("ping");
            var sendTask = _webSocket.SendAsync(
                new ArraySegment<byte>(pingBytes),
                WebSocketMessageType.Text,
                true,
                token
            );

            var completed = await Task.WhenAny(sendTask, Task.Delay(_heartbeatTimeout, token));
            if (completed != sendTask)
            {
                return false;
            }

            await sendTask;
            return true;
        }
        catch
        {
            SetState(LogPortClientState.Degraded);
            return false;
        }
    }


    private async Task EnsureSocketConnectedAsync(CancellationToken token)
    {
        if (_webSocket == null || _webSocket.State != WebSocketState.Open)
        {
            try
            {
                _webSocket?.Abort();
            }
            catch
            {
                // ignored
            }

            try
            {
                _webSocket?.Dispose();
            }
            catch
            {
                // ignored
            }

            var delay = TimeSpan.FromSeconds(1);
            var random = new Random();

            do
            {
                if (token.IsCancellationRequested)
                    return;
                _webSocket = _socketFactory?.Invoke() ?? throw new InvalidOperationException();
                try
                {
                    SetState(LogPortClientState.Connecting);
                    _logger?.Debug("Attempting WebSocket connection...");
                    await _webSocket.ConnectAsync(_serverUri, token).ConfigureAwait(false);
                    _logger?.Info("Connected to LogPort Agent");
                    _isAlive = true;
                    SetState(LogPortClientState.Connected);
                    return;
                }
                catch (Exception ex)
                {
                    _logger?.Warn($"Connection failed, retrying: {ex.Message}");
                    SetState(LogPortClientState.Disconnected);
                    _isAlive = false;
                    delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2 + random.Next(0, 500), _maxReconnectDelay.TotalMilliseconds));
                    try
                    {
                        await Task.Delay(delay, token);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                }

            } while (_webSocket.State != WebSocketState.Open && !token.IsCancellationRequested && _shouldReconnect);
        }
    }


    /// <summary>
    /// Asynchronously disposes the client and releases all resources.
    /// </summary>
    /// <returns>A task representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await _webSocket.CloseConnectionAsync(WebSocketCloseStatus.NormalClosure, "Client disposed",
            CancellationToken.None);
        await CastAndDispose(_cts);
        if (_senderTask != null) await CastAndDispose(_senderTask);

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
                await resourceAsyncDisposable.DisposeAsync();
            else
                resource.Dispose();
        }
    }
}