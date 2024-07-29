using System.Net;

namespace IntegrationTests.Extensions;

public static class HttpClientExtension
{
    public static async Task<HttpResponseMessage> GetAsyncWithRetry(this HttpClient client, string url, int retryCount = 5)
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