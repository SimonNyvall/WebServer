using Webserver;
using Microsoft.Extensions.Logging;

ILogger<Server> logger = LoggerFactory.Create(builder => {
    builder.AddConsole();
}).CreateLogger<Server>();

Server server = new(logger);

server.Start(config => {
    config.StaticFilePath = "static/index.html";
});

Console.ReadLine();