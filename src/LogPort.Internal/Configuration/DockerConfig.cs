namespace LogPort.Internal.Configuration;

public class DockerConfig
{
    /// <summary>
    /// Gets or sets whether to enable the Docker module.
    /// </summary>
    public bool Use { get; set; } = false;

    /// <summary>
    /// Gets or sets the Docker socket path.
    /// </summary>
    public string SocketPath { get; set; } = "unix:///var/run/docker.sock";

    /// <summary>
    /// Gets or sets whether the agent should monitor <b>EVERY</b> container in the host.
    /// </summary>
    /// <remarks>
    /// Depending on the environment, this can lead to excessive log throughput.
    /// It is recommended to use labels to mark which containers should be monitored
    /// </remarks>
    public bool WatchAllContainers { get; set; } = false;
}