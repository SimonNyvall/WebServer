using System.Diagnostics;

namespace IntegrationTests;

public class ServerTestHelper : IDisposable
{
    public string ServerUrl => "localhost";
    public int ServerPort => 6001;
    public string ServerAddress => $"http://{ServerUrl}:{ServerPort}";
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

    ~ServerTestHelper()
    {
        Dispose(false);
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
}