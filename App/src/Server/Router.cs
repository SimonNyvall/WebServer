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
    private readonly string _entryFile = "index.html";

    private readonly ILogger<Router> _logger;

    internal Router(string webSitePath, string entryFile)
    {
        _webSitePath = webSitePath;
        _entryFile = entryFile;

        _logger = LoggerFactory.Create(builder => {
            builder.AddConsole();
        }).CreateLogger<Router>();
    }

    internal void Respond(HttpListenerResponse response, ResponsePacket responsePacket)
    {
        response.ContentType = responsePacket.ContentType;
        response.ContentLength64 = responsePacket.Data!.Length;
        response.StatusCode = (int)responsePacket.Status;

        Stream output = response.OutputStream;
        output.Write(responsePacket.Data, 0, responsePacket.Data.Length);

        if (responsePacket.Redirect != string.Empty)
        {
            response.Redirect(responsePacket.Redirect!);
        }

        output.Close();
    }

    internal ResponsePacket RouteRequest(Verb verb, string path, Dictionary<string, string> urlParams)
    {
        if (path.EndsWith('/')) path = _entryFile;

        string extension = TryGetExtension(path);

        (bool success, ExtensionInfo extensionInfo) = TryGetExtensionInfo(extension);

        Method? route = methods.Find(method => method.Path == path && method.Verb == verb);

        ResponsePacket responsePacket = ResponsePacket.Empty();

        if (!success)
        {
            _logger.LogCritical("Failed to get extension info for {extension}", extension);

            path = "ErrorPages/UnkownType.html";
            responsePacket.SetStatus(Status.UnkownType);

            return ProcessResponse(extensionInfo, responsePacket, $"{_webSitePath}/{path}");
        }

        string fullPath = Path.Combine(_webSitePath, path.Trim('/'));

        _logger.LogInformation("Routing request for {fullPath}", fullPath);
        responsePacket = HandleRoute(urlParams, extensionInfo, route, responsePacket, fullPath);

        return responsePacket;
    }

    private ResponsePacket HandleRoute(Dictionary<string, string> urlParams, ExtensionInfo extensionInfo, Method? route, ResponsePacket responsePacket, string fullPath)
    {
        if (route is null) return ProcessResponse(extensionInfo, responsePacket, fullPath);

        string? redirect = route.HandleRequest(urlParams);

        if (redirect == string.Empty)
        {
            responsePacket = ProcessResponse(extensionInfo, responsePacket, fullPath);
            return responsePacket;
        }

        responsePacket.SetRedirect(redirect); 

        return responsePacket;
    }

    private ResponsePacket ProcessResponse(ExtensionInfo extensionInfo, ResponsePacket responsePacket, string fullPath)
    {
        responsePacket = extensionInfo.Loader switch
        {
            LoadType.ImageLoader => ImageLoader(fullPath, extensionInfo, responsePacket),
            LoadType.FileLoader => FileLoader(fullPath, extensionInfo, responsePacket),
            LoadType.PageLoader => PageLoader(fullPath, extensionInfo, responsePacket),
            _ => responsePacket
        };

        return responsePacket;
    }

    private string TryGetExtension(string path)
    {
        try
        {
            string[] parts = path.Split('.');

            if (parts.Length == 1) return string.Empty;

            return parts[^1];
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

    private ResponsePacket ImageLoader(string path, ExtensionInfo extensionInfo, ResponsePacket responsePacket)
    {
        FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new(fileStream);

        responsePacket
            .SetData(binaryReader.ReadBytes((int)fileStream.Length))
            .SetContentType(extensionInfo.ContentType);

        binaryReader.Close();
        fileStream.Close();

        return responsePacket;
    }

    private ResponsePacket PageLoader(string path, ExtensionInfo extensionInfo, ResponsePacket responsePacket)
    {
        try
        {
            responsePacket = Loader(path, extensionInfo, responsePacket);
        }
        catch
        {
            responsePacket.SetStatus(Status.PageNotFound);

            _logger.LogCritical("Failed to load page {path}", path);
        }

        return responsePacket;
    }

    private ResponsePacket FileLoader(string path, ExtensionInfo extensionInfo, ResponsePacket responsePacket)
    {
        try
        {
            responsePacket = Loader(path, extensionInfo, responsePacket);
        }
        catch
        {
            responsePacket.SetStatus(Status.FileNotFound);

            _logger.LogCritical("Failed to load file {path}", path);
        }

        return responsePacket;
    }

    private static ResponsePacket Loader(string path, ExtensionInfo extensionInfo, ResponsePacket responsePacket)
    {
        string text = string.Empty;

        try
        {
            text = File.ReadAllText(path);
        }
        catch
        {
            throw new Exception("Failed to read file");
        }

        responsePacket
            .SetData(Encoding.UTF8.GetBytes(text))
            .SetContentType(extensionInfo.ContentType)
            .SetStatus(Status.OK);

        return responsePacket;
    }
}