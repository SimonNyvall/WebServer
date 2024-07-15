namespace Webserver;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;
using Webserver.Models;

public class Server
{
    private static readonly Configuration _configuration = new();
    private static Semaphore? _semaphore;

    private readonly ILogger<Server> _logger;

    public Server()
    {
        _logger = LoggerFactory.Create(builder => {
            builder.AddConsole();
        }).CreateLogger<Server>();
    }

    public void Start(Action<Configuration>? configure)
    {
        SetUpConfiguration(configure);

        Router router = new(_configuration.StaticFilePath);

        IPAddress[] localhostIPs = GetLocalHostIPs();
        HttpListener listener = GetInitializedListener(localhostIPs);

        _logger.LogInformation("Starting server");

        listener.Start();
        _logger.LogInformation("Server started");

        _ = Task.Run(() => RunServer(listener, router));
    }

    private static void SetUpConfiguration(Action<Configuration>? configure)
    {
        if (configure is null) return;
        // TODO: Implement configuration customation
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
            _logger.LogInformation("Listening on {ip}:{port}", ip, port);
            listener.Prefixes.Add($"http://{ip}:{port}/");
        }

        return listener;
    }

    private async Task RunServer(HttpListener listener, Router router)
    {
        while (true)
        {
            _semaphore ??= new Semaphore(_configuration.MaxSimultaneousConnections, _configuration.MaxSimultaneousConnections);

            _semaphore.WaitOne();
            await StartConnectionListener(listener, router);
        }
    }

    private async Task StartConnectionListener(HttpListener listener, Router router)
    {
        HttpListenerContext context = await listener.GetContextAsync();

        _semaphore!.Release();

        string path = (context.Request.Url ?? new Uri("http://localhost")).LocalPath;

        ResponsePacket? responsePacket = router.RouteRequest(path);
        
        if (responsePacket is not null)
        {
            router.OKRespond(context.Response, responsePacket);
        }
        else
        {
            _logger.LogWarning("Not Found: {path}", path);
            router.ErrorRespond(context.Response, HttpStatusCode.NotFound, "Not Found");
        }
    }
}