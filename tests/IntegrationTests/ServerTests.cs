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
        var response = await client.GetAsync(serverHelper.ServerUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public class ServerTestHelper : IDisposable
{
    public string ServerUrl => "http://localhost:6001";
    private bool _disposed = false;
    private Process? _serverProcess;

    public ServerTestHelper() 
    { 
        string workingDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "src"));

        ProcessStartInfo startInfo = new()
        {
            FileName = "dotnet",
            Arguments = "run",
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Task.Run(() => StartServer(startInfo));
    }

    private Task StartServer(ProcessStartInfo startInfo)
    {
        _serverProcess = Process.Start(startInfo);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (!disposing) return;

        if (_serverProcess is not null && !_serverProcess.HasExited)
        {
            _serverProcess.Kill();
            _serverProcess.WaitForExit();
            _serverProcess.Dispose();
        }

        _disposed = true;
    }

    ~ServerTestHelper()
    {
        Dispose(false);
    }
}