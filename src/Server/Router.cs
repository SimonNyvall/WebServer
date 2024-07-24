namespace Webserver;

using System.IO;
using System.Net;
using System.Text;
using Webserver.Models;
using Microsoft.Extensions.Logging;

public class Router
{
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

    public Router(string webSitePath)
    {
        _webSitePath = webSitePath;

        _logger = LoggerFactory.Create(builder => {
            builder.AddConsole();
        }).CreateLogger<Router>();
    }

    public ResponsePacket RouteRequest(string path)
    {
        if (path.EndsWith('/')) path = "index.html";

        string extension = GetExtension(path);

        (bool success, ExtensionInfo data) extensionInfo = TryGetExtensionInfo(extension);

        ResponsePacket responsePacket = ResponsePacket.BadRequest();

        if (!extensionInfo.success)
        {
            responsePacket = new ResponsePacket() { Status = Status.UnkownType };
        }

        string fullPath = Path.Combine(_webSitePath, path.Replace("/", string.Empty));

        _logger.LogInformation("Routing request for {fullPath}", fullPath);

        responsePacket = extensionInfo.data.Loader switch
        {
            LoadType.ImageLoader => ImageLoader(fullPath, extensionInfo.data),
            LoadType.FileLoader => FileLoader(fullPath, extensionInfo.data),
            LoadType.PageLoader => PageLoader(fullPath, extensionInfo.data),
            _ => responsePacket
        };

        return responsePacket;
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

    public void OKRespond(HttpListenerResponse response, ResponsePacket responsePacket)
    {
        response.ContentType = responsePacket.ContentType;
        response.ContentLength64 = responsePacket.Data.Length;

        Stream output = response.OutputStream;
        output.Write(responsePacket.Data, 0, responsePacket.Data.Length);

        output.Close();
    }

    public void ErrorRespond(HttpListenerResponse response, ResponsePacket responsePacket)
    {
        response.Redirect($"http://{responsePacket.Redirect}"); // TODO: fetch the ip and check for https

        Stream output = response.OutputStream;

        output.Close();
    }
}