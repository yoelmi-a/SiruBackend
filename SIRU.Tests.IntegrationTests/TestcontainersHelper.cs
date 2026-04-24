using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using SIRU.Infrastructure.Persistence.Contexts;
using Testcontainers.PostgreSql;

namespace SIRU.Tests.IntegrationTests;

public sealed class TestcontainersHelper : IDisposable
{
    private PostgreSqlContainer? _container;
    private string? _connectionString;

    public string ConnectionString => _connectionString
        ?? throw new InvalidOperationException("Container not started. Call StartAsync first.");

    public async Task StartAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("sirus_test")
            .WithUsername("sirus_user")
            .WithPassword("testpassword")
            .WithCleanUp(true)
            .Build();

        await _container.StartAsync();

        _connectionString = _container.GetConnectionString();
    }

    public async Task MigrateDatabaseAsync()
    {
        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("Container not started.");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();
    }

    public void Dispose()
    {
        _container?.DisposeAsync().AsTask().Dispose();
    }
}