namespace Webserver;

public class Configuration
{
    public int MaxSimultaneousConnections { get; set; } = 20;
    public string FallbackHtml { get; set; } = "<html><body><h1>An error has occurred</h1></body></html>";
    public int Port { get; set; } = 6001;
    public string Ip { get; set; } = "localhost";
    public string StaticFilePath { get; set; } = "wwwroot";
    public bool UseHttps { get; set; } = false;
    public string EntryFile { get; set; } = "index.html";
}