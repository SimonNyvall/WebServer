using System.Net;

namespace IntegrationTests;

public class ServerTests
{
    [Fact]
    public async Task EnsureServer_RespondsWith_OK()
    {
        using var serverHelper = new ServerTestHelper();

        var client = new HttpClient();

        var response = await GetAsyncWithRetry(client, serverHelper.ServerAddress);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EnsureServer_RespondsWith_NotFound()
    {
        using var serverHelper = new ServerTestHelper();

        var client = new HttpClient();

        var response = await GetAsyncWithRetry(client, $"{serverHelper.ServerAddress}/notfound");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // After redirecting to /404 page, should retrun /200
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
        Assert.True(response.Content.Headers.ContentLength > 0);
        Assert.Contains("Page not found", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task EnsureServer_RespondsWith_FileNotFound()
    {
        using var serverHelper = new ServerTestHelper();

        var client = new HttpClient();

        var response = await GetAsyncWithRetry(client, $"{serverHelper.ServerAddress}/foo.bar");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // After redirecting to /510 page, should retrun /200
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
        Assert.True(response.Content.Headers.ContentLength > 0);
        Assert.Contains("Unkown type", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task EnsureServer_RespondsWith_PageNotFound()
    {
        using var serverHelper = new ServerTestHelper();

        var client = new HttpClient();

        var response = await GetAsyncWithRetry(client, $"{serverHelper.ServerAddress}/foo.html");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // After redirecting to /400 page, should retrun /200
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
        Assert.True(response.Content.Headers.ContentLength > 0);
        Assert.Contains("Page not found", await response.Content.ReadAsStringAsync());
    }

    private async Task<HttpResponseMessage> GetAsyncWithRetry(HttpClient client, string url, int retryCount = 5)
    {
        HttpResponseMessage? response = null;

        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                response = await client.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.OK) break;
            }
            catch
            {
                await Task.Delay(1000);
            }
        }

        return response ?? throw new Exception("Failed to get response");
    }
 }

