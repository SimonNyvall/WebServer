using System.Text;

namespace Webserver.Models;

public class ResponsePacket
{
    public byte[]? Data { get; private set; }
    public string? ContentType { get; private set; }
    public Status Status { get; private set; }
    public string? Redirect { get; private set; }

    public ResponsePacket(
        byte[]? data,
        string? contentType,
        Status status,
        string? redirect
    )
    {
        SetData(data);
        SetContentType(contentType);
        SetStatus(status);
        SetRedirect(redirect);
    }

    public ResponsePacket SetData(byte[]? data)
    {
        data ??= Encoding.UTF8.GetBytes(string.Empty);

        Data = data;

        return this;
    }

    public ResponsePacket SetContentType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType)) contentType = "text/html";

        ContentType = contentType;

        return this;
    }

    public ResponsePacket SetStatus(Status status)
    {
        Status = status;

        return this;
    }

    public ResponsePacket SetRedirect(string? redirect)
    {
        if (string.IsNullOrEmpty(redirect)) redirect = string.Empty;

        Redirect = redirect;

        return this;
    }

    public static ResponsePacket Empty() => new([], "text/html", Status.OK, string.Empty);
}