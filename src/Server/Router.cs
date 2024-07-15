namespace Webserver;

using System.IO;
using System.Net;
using System.Text;
using Webserver.Models;

public class Router(string webSitePath)
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

    public ResponsePacket RouteRequest(string path)
    {
        if (path.EndsWith('/')) path = "index.html";

        string extension = path.Split('.')[^1];

        ResponsePacket responsePacket = ResponsePacket.BadRequest();

        if (_extensionMap.TryGetValue(extension, out ExtensionInfo extensionInfo))
        {
            string fullPath = Path.Combine(webSitePath, path.Replace("/", string.Empty));

            responsePacket = extensionInfo.Loader switch
            {
                LoadType.ImageLoader => ImageLoader(fullPath, extensionInfo),
                LoadType.FileLoader => FileLoader(fullPath, extensionInfo),
                LoadType.PageLoader => PageLoader(fullPath, extensionInfo),
                _ => responsePacket
            };
        }

        return responsePacket;
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

    public void ErrorRespond(HttpListenerResponse response, HttpStatusCode statusCode, string message)
    {
        response.StatusCode = (int)statusCode;
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        response.ContentLength64 = buffer.Length;

        Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);

        output.Close();
    }
}