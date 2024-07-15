namespace Webserver.Models;

public class ResponsePacket
{
    public byte[] Data { get; set; }
    public string ContentType { get; set; }
}