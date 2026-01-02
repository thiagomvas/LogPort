namespace LogPort.Internal.Configuration;

public class FileTailingConfiguration
{
    public string ServiceName { get; set; }
    public string Path { get; set; }

    public FileTailingConfiguration(string serviceName, string path)
    {
        ServiceName = serviceName;
        Path = path;
    }
}