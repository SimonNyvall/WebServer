using App.Server.Models;
using Microsoft.Extensions.Logging;

namespace App.Server;

public static class LoggerMessageDefinitions
{
    private static readonly Action<ILogger, Exception?> StartingServerMessageDefinition =
        LoggerMessage.Define(LogLevel.Information, new EventId(1, nameof(Server)), "Starting server");

    private static readonly Action<ILogger, Exception?> ServerStartedMessageDefinition =
        LoggerMessage.Define(LogLevel.Information, new EventId(2, nameof(Server)), "Server started");

    private static readonly Action<ILogger, string, int, Exception?> ListeningOnMessageDefinition =
        LoggerMessage.Define<string, int>(LogLevel.Information, new EventId(3, nameof(Server)), "Listening on {ip}:{port}");

    private static readonly Action<ILogger, Status, string, Exception?> ErrorPathMessageDefinition =
        LoggerMessage.Define<Status, string>(LogLevel.Critical, new EventId(4, nameof(Server)), "Error: {statusCode} {pagePath}");

    private static readonly Action<ILogger, string, Exception?> ExtensionErrorMessageDefinition =
        LoggerMessage.Define<string>(LogLevel.Critical, new EventId(5, nameof(Router)), "Failed to get extension info for {extension}");

    private static readonly Action<ILogger, string, Exception?> RoutingRequestMessageDefinition =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(6, nameof(Router)), "Routing request for {fullPath}");

    private static readonly Action<ILogger, string, Exception?> FailedToGetPathMessageDefinition =
        LoggerMessage.Define<string>(LogLevel.Critical, new EventId(7, nameof(Router)), "Failed to get extension for {path}");

    private static readonly Action<ILogger, string, Exception?> FailedToLoadPageMessageDefinition =
        LoggerMessage.Define<string>(LogLevel.Critical, new EventId(7, nameof(Router)), "Failed to load page for {path}");

    private static readonly Action<ILogger, string, Exception?> FailedToLoadFileMessageDefinition =
        LoggerMessage.Define<string>(LogLevel.Critical, new EventId(7, nameof(Router)), "Failed to load file for {path}");

    public static void LogStartingServerMessage(this ILogger logger) =>
        StartingServerMessageDefinition(logger, null);

    public static void LogServerStartedMessage(this ILogger logger) =>
        ServerStartedMessageDefinition(logger, null);

    public static void LogListeningOnMessage(this ILogger logger, string ip, int port) =>
        ListeningOnMessageDefinition(logger, ip, port, null);

    public static void LogErrorPathMessage(this ILogger logger, Status status, string pagePath) =>
        ErrorPathMessageDefinition(logger, status, pagePath, null);

    public static void LogExtensionErrorMessage(this ILogger logger, string extension) =>
        ExtensionErrorMessageDefinition(logger, extension, null);

    public static void LogRoutingRequestMessage(this ILogger logger, string fullPath) =>
        RoutingRequestMessageDefinition(logger, fullPath, null);

    public static void LogFailedToGetPathMessage(this ILogger logger, string path) =>
        FailedToGetPathMessageDefinition(logger, path, null);

    public static void LogFailedToLoadPageMessage(this ILogger logger, string path) =>
        FailedToLoadPageMessageDefinition(logger, path, null);

    public static void LogFailedToLoadFileMessage(this ILogger logger, string path) =>
        FailedToLoadFileMessageDefinition(logger, path, null);
}
