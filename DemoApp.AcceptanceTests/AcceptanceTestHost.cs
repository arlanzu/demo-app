using DemoApp.Infrastructure.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DemoApp.AcceptanceTests;

internal sealed class AcceptanceTestHost : IDisposable
{
    private readonly string _databasePath;
    private readonly AcceptanceWebApplicationFactory _factory;

    private AcceptanceTestHost(string databasePath, AcceptanceWebApplicationFactory factory)
    {
        _databasePath = databasePath;
        _factory = factory;
        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public HttpClient Client { get; }

    public static AcceptanceTestHost Create()
    {
        var directory = Path.Combine(Path.GetTempPath(), "DemoApp.AcceptanceTests");
        Directory.CreateDirectory(directory);

        var databasePath = Path.Combine(directory, $"acceptance-{Guid.NewGuid():N}.db");
        var factory = new AcceptanceWebApplicationFactory(databasePath);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        return new AcceptanceTestHost(databasePath, factory);
    }

    public void Dispose()
    {
        Client.Dispose();
        _factory.Dispose();

        try
        {
            if (File.Exists(_databasePath))
            {
                File.Delete(_databasePath);
            }
        }
        catch (IOException)
        {
        }
    }

    private sealed class AcceptanceWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databasePath;

        public AcceptanceWebApplicationFactory(string databasePath)
        {
            _databasePath = databasePath;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source={_databasePath}"
                });
            });
            builder.ConfigureServices(services =>
            {
                services.AddDataProtection()
                    .UseEphemeralDataProtectionProvider();
            });
        }
    }
}
