using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LogPort.Core.Models;

namespace LogPort.SDK;

/// <summary>
/// Represents a client for sending structured logs to a LogPort server over WebSocket.
/// </summary>
/// <remarks>
/// Supports asynchronous, non-blocking logging with an internal queue. Use <see cref="EnsureConnectedAsync"/> 
/// before sending logs. Call <see cref="FlushAsync"/> to ensure all queued logs are sent before shutdown.
/// Implements <see cref="IDisposable"/> to clean up WebSocket and cancellation resources.
/// </remarks>
public sealed class LogPortClient : IDisposable
{
    private readonly Uri _serverUri;
    private ClientWebSocket _webSocket;
    private readonly ConcurrentQueue<LogEntry> _messageQueue;
    private readonly CancellationTokenSource _cts;
    private Task? _senderTask;
    private readonly TimeSpan _maxReconnectDelay = TimeSpan.FromSeconds(30);
    private readonly TimeSpan _heartbeatTimeout = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _heartbeatInterval = TimeSpan.FromSeconds(10);

    
    private const int SendDelayMs = 50;

    private LogPortClient(string serverUrl)
    {
        _serverUri = new Uri(serverUrl);
        _webSocket = new ClientWebSocket();
        _messageQueue = new ConcurrentQueue<LogEntry>();
        _cts = new CancellationTokenSource();
    }

    public LogPortClient(LogPortConfig config) : this(config.AgentUrl)
    {
        
    }

    /// <summary>
    /// Creates a <see cref="LogPortClient"/> using the server URL specified in the environment variable LOGPORT_SERVER_URL.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the environment variable is not set.</exception>
    public static LogPortClient FromEnvironment()
    {
        var config = LogPortConfig.LoadFromEnvironment();

        return new LogPortClient(config.AgentUrl);
    }

    /// <summary>
    /// Creates a <see cref="LogPortClient"/> with a specific server URL.
    /// </summary>
    /// <param name="serverUrl">The WebSocket URL of the LogPort server (e.g., ws://localhost:5000/logs).</param>
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
        _senderTask = Task.Run(() => ProcessQueueAsync(_cts.Token), token);
    }


    /// <summary>
    /// Enqueues a <see cref="LogEntry"/> to be sent asynchronously to the server.
    /// </summary>
    /// <param name="entry">The log entry to send.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.</exception>
    public void Log(LogEntry entry)
    {
        if (entry is null) throw new ArgumentNullException(nameof(entry));
        _messageQueue.Enqueue(entry);
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
            if (_messageQueue.TryDequeue(out var entry))
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

                }
                catch (Exception ex)
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
    /// Waits until all queued logs have been sent to the server.
    /// </summary>
    public async Task FlushAsync()
    {
        while (!_messageQueue.IsEmpty)
            await Task.Delay(SendDelayMs).ConfigureAwait(false);
    }

    /// <summary>
    /// Stops background processing, cancels pending sends, and releases all resources.
    /// </summary>
    /// <remarks>
    /// Should be called when the client is no longer needed. After disposal, the client cannot be reused.
    /// </remarks>
    public void Dispose()
    {
        _cts.Cancel();
        _senderTask?.Wait();
        _webSocket.Dispose();
        _cts.Dispose();
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
            return false; 
        }
    }
    
    
    private async Task EnsureSocketConnectedAsync(CancellationToken token)
    {
        if (_webSocket == null || _webSocket.State != WebSocketState.Open)
        {
            try { _webSocket?.Abort(); } catch { }
            try { _webSocket?.Dispose(); } catch { }

            var delay = TimeSpan.FromSeconds(1);
            var random = new Random();

            while (_webSocket.State != WebSocketState.Open && !token.IsCancellationRequested)
            {
                _webSocket = new ClientWebSocket();
                try
                {
                    await _webSocket.ConnectAsync(_serverUri, token).ConfigureAwait(false);
                    return;
                }
                catch (Exception ex)
                {
                    await Task.Delay(delay + TimeSpan.FromMilliseconds(random.Next(0, 500)), token);
                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, _maxReconnectDelay.TotalSeconds));
                }
            }
        }
    }



}