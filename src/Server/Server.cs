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

        configure(_configuration);
    }

    private static IPAddress[] GetLocalHostIPs()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

        return [.. host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)];
    }

    private HttpListener GetInitializedListener(IPAddress[] localhostIPs)
    {
        HttpListener listener = new();

        int port = _configuration.Port;

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

        Verb verb = Method.MapStringToVerb(context.Request.HttpMethod);

        string path = (context.Request.Url ?? new Uri($"{(_configuration.UseHttps ? "https://" : "http://")}{_configuration.Ip}")).LocalPath;

        Dictionary<string, string> urlParameters = GetUrlParameters(context.Request.Url?.ToString() ?? string.Empty);

        ResponsePacket responsePacket = router.RouteRequest(verb, path, urlParameters);

        if (responsePacket.Status != Status.OK) // TODO: this is ugly
        {
            string pagePath = GetErrorPageRedirectPath(responsePacket.Status);
            responsePacket.SetRedirect(pagePath);

            _logger.LogCritical("Error: {statusCode} {pagePath}", responsePacket.Status, pagePath);

            context.Response.StatusCode = (int)responsePacket.Status;
            router.Respond(context.Response, responsePacket);
        }
        else
        {
            router.Respond(context.Response, responsePacket);
        }
    }

    private Dictionary<string, string> GetUrlParameters(string url)
    {
        string[] urlParts = url.Split('?');

        if (urlParts.Length < 2) return [];

        string[] parameters = urlParts[1].Split('&');

        return parameters.Select(parameter => parameter.Split('=')).ToDictionary(parameter => parameter[0], parameter => parameter[1]);
    }

    private string GetErrorPageRedirectPath(Status errorType)
    {
        return errorType switch
        {
            Status.ExpiredSession => "/ErrorPages/ExpiredSession.html",
            Status.NotAuthorized =>  "/ErrorPages/NotAuthorized.html",
            Status.FileNotFound =>   "/ErrorPages/FileNotFound.html",
            Status.PageNotFound =>   "/ErrorPages/PageNotFound.html",
            Status.ServerError =>    "/ErrorPages/ServerError.html",
            Status.UnkownType =>     "/ErrorPages/UnkownType.html",
            _ => throw new NotImplementedException("Failed to map redirect path")
        };
    }
}