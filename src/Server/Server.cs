namespace Webserver;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

public class Server
{
    private static readonly Configuration _configuration = new();
    private static Semaphore? _semaphore;

    private readonly ILogger<Server> _logger;

    public Server(ILogger<Server> logger)
    {
        _logger = logger;
    }

    public async Task Start(Action<Configuration>? configure)
    {
        SetUpConfiguration(configure);

        IPAddress[] localhostIPs = GetLocalHostIPs();
        HttpListener listener = GetInitializedListener(localhostIPs);

        _logger.LogInformation("Starting server");

        listener.Start();
        _logger.LogInformation("Server started");

        string html = await GetHtmlContent(_configuration.HtmlPath);

        _ = Task.Run(() => RunServer(listener, html));
    }

    private static void SetUpConfiguration(Action<Configuration>? configure)
    {
        if (configure is null) return;
        // TODO: Implement configuration customation
    }

    private async Task<string> GetHtmlContent(string htmlPath)
    {
        if (!File.Exists(htmlPath))
        {
            _logger.LogError("File {htmlPath} not found", htmlPath);

            return _configuration.DefaultHtml;
        }

        return await File.ReadAllTextAsync(htmlPath);
    }

    private static IPAddress[] GetLocalHostIPs()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

        return [.. host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)];
    }

    private HttpListener GetInitializedListener(IPAddress[] localhostIPs)
    {
        HttpListener listener = new();

        int port = _configuration.DefaultPort;

        listener.Prefixes.Add($"http://localhost:{port}/");

        foreach (IPAddress ip in localhostIPs)
        {
            _logger.LogInformation("Listening on {ip}", ip);
            listener.Prefixes.Add($"http://{ip}/");
        }

        return listener;
    }

    private static async Task RunServer(HttpListener listener, string html)
    {
        while (true)
        {
            _semaphore ??= new Semaphore(_configuration.MaxSimultaneousConnections, _configuration.MaxSimultaneousConnections);

            _semaphore.WaitOne();
            await StartConnectionListener(listener, html);
        }
    }

    private static async Task StartConnectionListener(HttpListener listener, string html)
    {
        HttpListenerContext context = await listener.GetContextAsync();

        _semaphore!.Release();

        byte[] buffer = Encoding.UTF8.GetBytes(html);
        
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.Close();
    }
}