using System.Net;

namespace IntegrationTests;

public class ServerTests
{
    [Fact]
    public async Task EnsureServer_RespondsWith_OK()
    {
        using var serverHelper = new ServerTestHelper();

        var client = new HttpClient();

        Thread.Sleep(5000);

        var response = await client.GetAsync(serverHelper.ServerAddress);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EnsureServer_RespondsWith_NotFound()
    {
        using var serverHelper = new ServerTestHelper();

        var client = new HttpClient();

        Thread.Sleep(5000);

        var response = await client.GetAsync($"{serverHelper.ServerAddress}/notfound");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
 }

