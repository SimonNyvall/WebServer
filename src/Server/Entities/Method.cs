namespace Webserver.Models;

public enum Verb
{
    GET,
    POST,
    PUT,
    DELETE,
    OPTIONS,
    HEAD,
    TRACE,
    CONNECT
}

public class Method
{
    public Verb Verb { get; set; }
    public string Path { get; set; } = string.Empty;
    public Func<Dictionary<string, string>, string> HandleRequest { get; set; } = _ => string.Empty;

    public static string GetVerbToLower(Verb verb)
    {
        return verb.ToString().ToLower();
    }
}