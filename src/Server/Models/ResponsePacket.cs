namespace Webserver.Models;

using System.Text;

public class ResponsePacket
{
    public byte[] Data { get; set; } = [];
    public string ContentType { get; set; } = string.Empty;

    public static ResponsePacket BadRequest()
    {
        return new ResponsePacket { Data = Encoding.UTF8.GetBytes("Bad Request"), ContentType = "text/plain" };
    }
}