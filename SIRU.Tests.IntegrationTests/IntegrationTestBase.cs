using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SIRU.Infrastructure.Persistence.Contexts;

namespace SIRU.Tests.IntegrationTests;

public abstract class IntegrationTestBase : WebApplicationFactory<Program>, IAsyncDisposable
{
    private readonly TestcontainersHelper _container;
    private string _connectionString = string.Empty;

    protected IntegrationTestBase()
    {
        _container = new TestcontainersHelper();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _container.StartAsync().Wait();
        _connectionString = _container.ConnectionString;

        _container.MigrateDatabaseAsync().Wait();
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_connectionString));
        });
    }

    public async ValueTask DisposeAsync()
    {
        _container.Dispose();
        await base.DisposeAsync();
    }
}