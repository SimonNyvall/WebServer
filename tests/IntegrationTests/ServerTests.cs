using System.Net;
using IntegrationTests.Extensions;

namespace IntegrationTests;

public class ServerTests
{
    [Fact]
    public async Task EnsureServer_RespondsWith_OK()
    {
        using ServerTestHelper serverHelper = new();
        HttpClient client = new();

        var response = await client.GetAsyncWithRetry(serverHelper.ServerAddress);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
        Assert.True(response.Content.Headers.ContentLength > 0);
    }

    [Fact]
    public async Task EnsureServer_RespondsWith_NotFound()
    {
        using ServerTestHelper serverHelper = new();
        HttpClient client = new();

        var response = await client.GetAsyncWithRetry($"{serverHelper.ServerAddress}/notfound");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // After redirecting to /404 page, should retrun /200
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
        Assert.True(response.Content.Headers.ContentLength > 0);
        Assert.Contains("Page not found", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task EnsureServer_RespondsWith_FileNotFound()
    {
        using ServerTestHelper serverHelper = new();
        HttpClient client = new();

        var response = await client.GetAsyncWithRetry($"{serverHelper.ServerAddress}/foo.bar");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // After redirecting to /510 page, should retrun /200
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
        Assert.True(response.Content.Headers.ContentLength > 0);
        Assert.Contains("Unkown type", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task EnsureServer_RespondsWith_PageNotFound()
    {
        using ServerTestHelper serverHelper = new();
        HttpClient client = new();

        var response = await client.GetAsyncWithRetry($"{serverHelper.ServerAddress}/foo.html");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // After redirecting to /400 page, should retrun /200
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
        Assert.True(response.Content.Headers.ContentLength > 0);
        Assert.Contains("Page not found", await response.Content.ReadAsStringAsync());
    }
}

