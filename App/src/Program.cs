using Webserver;

Server server = new();

server.Start(config => {
    config.StaticFilePath = "wwwroot";
});

Console.ReadLine();