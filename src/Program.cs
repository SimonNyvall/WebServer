using Webserver;
using Microsoft.Extensions.Logging;

ILogger<Server> logger = LoggerFactory.Create(builder => {
    builder.AddConsole();
}).CreateLogger<Server>();

Server server = new(logger);

await server.Start(config => {
    config.HtmlPath = "static/index.html";
});

Console.ReadLine();