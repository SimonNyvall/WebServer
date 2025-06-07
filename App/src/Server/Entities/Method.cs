namespace App.Server.Models;

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

    public static Verb MapStringToVerb(string verb)
    {
        return verb switch
        {
            "GET" => Verb.GET,
            "POST" => Verb.POST,
            "PUT" => Verb.PUT,
            "DELETE" => Verb.DELETE,
            "OPTIONS" => Verb.OPTIONS,
            "HEAD" => Verb.HEAD,
            "TRACE" => Verb.TRACE,
            "CONNECT" => Verb.CONNECT,
            _ => throw new NotImplementedException("Failed to map verb")
        };
    }
}