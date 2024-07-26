using System.Diagnostics;
using System.Net;
using Webserver;

namespace IntegrationTests;

public class ServerTests
{
    [Fact]
    public async Task EnsureServer_RespondsWith_OK()
    {
        using var serverHelper = new ServerTestHelper();

        var client = new HttpClient();
        var response = await client.GetAsync(serverHelper.ServerAddress);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
 }

