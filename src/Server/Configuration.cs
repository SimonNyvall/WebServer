namespace Webserver;

public class Configuration
{
    public int MaxSimultaneousConnections { get; set; } = 20;
    public string DefaultHtml { get; set; } = "<html><body><h1>Hello, World!</h1></body></html>";
    public int DefaultPort { get; set; } = 6001;
    public string DefaultIp { get; set; } = "localhost";
    public string StaticFilePath { get; set; } = "static";
}