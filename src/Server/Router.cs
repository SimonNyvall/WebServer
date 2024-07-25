namespace Webserver;

using System.IO;
using System.Net;
using System.Text;
using Webserver.Models;
using Microsoft.Extensions.Logging;

public class Router
{
    internal List<Method> methods { get; set; } = [];

    private Dictionary<string, ExtensionInfo> _extensionMap = new()
    {
        {"ico", new ExtensionInfo() {Loader=LoadType.ImageLoader, ContentType="image/ico"}},
        {"png", new ExtensionInfo() {Loader=LoadType.ImageLoader, ContentType="image/png"}},
        {"jpg", new ExtensionInfo() {Loader=LoadType.ImageLoader, ContentType="image/jpg"}},
        {"gif", new ExtensionInfo() {Loader=LoadType.ImageLoader, ContentType="image/gif"}},
        {"bmp", new ExtensionInfo() {Loader=LoadType.ImageLoader, ContentType="image/bmp"}},
        {"html", new ExtensionInfo() {Loader=LoadType.PageLoader, ContentType="text/html"}},
        {"css", new ExtensionInfo() {Loader=LoadType.FileLoader, ContentType="text/css"}},
        {"js", new ExtensionInfo() {Loader=LoadType.FileLoader, ContentType="text/javascript"}},
        {"", new ExtensionInfo() {Loader=LoadType.PageLoader, ContentType="text/html"}},
    };

    private readonly string _webSitePath = string.Empty;

    private readonly ILogger<Router> _logger;

    internal Router(string webSitePath)
    {
        _webSitePath = webSitePath;

        _logger = LoggerFactory.Create(builder => {
            builder.AddConsole();
        }).CreateLogger<Router>();
    }

    internal ResponsePacket RouteRequest(Verb verb, string path, Dictionary<string, string> urlParams)
    {
        if (path.EndsWith('/')) path = "index.html"; // TODO: meka a default index.html page and set path to the confifuted page instead.

        string extension = GetExtension(path);

        (bool success, ExtensionInfo value) extensionInfo = TryGetExtensionInfo(extension);

        Method? route = methods.Find(method => method.Path == path && method.Verb == verb);

        ResponsePacket responsePacket = ResponsePacket.BadRequest();

        if (!extensionInfo.success)
        {
            _logger.LogCritical("Failed to get extension info for {extension}", extension);
            return responsePacket;
        }

        string fullPath = Path.Combine(_webSitePath, path.Replace("/", string.Empty));

        _logger.LogInformation("Routing request for {fullPath}", fullPath);
        responsePacket = HandleRoute(urlParams, extensionInfo.value, route, responsePacket, fullPath);

        return responsePacket;

        ResponsePacket ProcessResponse(ExtensionInfo extensionInfo, ResponsePacket responsePacket, string fullPath)
        {
            responsePacket = extensionInfo.Loader switch
            {
                LoadType.ImageLoader => ImageLoader(fullPath, extensionInfo),
                LoadType.FileLoader => FileLoader(fullPath, extensionInfo),
                LoadType.PageLoader => PageLoader(fullPath, extensionInfo),
                _ => responsePacket
            };
            return responsePacket;
        }

        ResponsePacket HandleRoute(Dictionary<string, string> urlParams, ExtensionInfo extensionInfo, Method? route, ResponsePacket responsePacket, string fullPath)
        {
            if (route is null) return ProcessResponse(extensionInfo, responsePacket, fullPath);

            string? redirect = route.HandleRequest(urlParams);

            if (redirect == string.Empty)
            {
                responsePacket = ProcessResponse(extensionInfo, responsePacket, fullPath);
                return responsePacket;
            }
                
            responsePacket.Redirect = redirect;

            return responsePacket;
        }
    }

    private string GetExtension(string path)
    {
        try
        {
            return path.Split('.')[^1];
        }
        catch (IndexOutOfRangeException)
        {
            _logger.LogCritical("Failed to get extension for {path}", path);
            return string.Empty;
        }
    }

    private (bool success, ExtensionInfo data) TryGetExtensionInfo(string extension)
    {
        if (!_extensionMap.TryGetValue(extension, out ExtensionInfo extensionInfo))
        {
            _logger.LogCritical("Failed to get extension info for {extension}", extension);
            return (false, new ExtensionInfo());
        }

        return (true, extensionInfo);
    }

    private ResponsePacket ImageLoader(string path, ExtensionInfo extensionInfo)
    {
        FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new(fileStream);

        ResponsePacket responsePacket = new() { Data = binaryReader.ReadBytes((int)fileStream.Length), ContentType = extensionInfo.ContentType };

        binaryReader.Close();
        fileStream.Close();

        return responsePacket;
    }

    private ResponsePacket PageLoader(string path, ExtensionInfo extensionInfo)
    {
        ResponsePacket responsePacket = FileLoader(path, extensionInfo);
        
        return responsePacket;
    }

    private ResponsePacket FileLoader(string path, ExtensionInfo extensionInfo)
    {
        string text = File.ReadAllText(path);

        ResponsePacket responsePacket = new() { Data = Encoding.UTF8.GetBytes(text), ContentType = extensionInfo.ContentType };

        return responsePacket;
    }

    internal void OKRespond(HttpListenerResponse response, ResponsePacket responsePacket)
    {
        response.ContentType = responsePacket.ContentType;
        response.ContentLength64 = responsePacket.Data.Length;

        Stream output = response.OutputStream;
        output.Write(responsePacket.Data, 0, responsePacket.Data.Length);

        output.Close();
    }

    internal void ErrorRespond(HttpListenerResponse response, ResponsePacket responsePacket)
    {
        response.Redirect($"http://{responsePacket.Redirect}"); // TODO: fetch the ip and check for https

        Stream output = response.OutputStream;

        output.Close();
    }
}