namespace Webserver.Models;

public enum Status : int
{
    OK = 200,
    ExpiredSession = 401,
    NotAuthorized = 403,
    FileNotFound = 404,
    PageNotFound = 400,
    ServerError = 500,
    UnkownType = 510
}