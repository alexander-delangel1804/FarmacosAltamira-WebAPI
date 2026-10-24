using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;

namespace FarmacosAltamira.Infrastructure.HealthChecks;

public sealed class DatabaseReadyHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public DatabaseReadyHealthCheck(IOptions<DatabaseHealthCheckOptions> options)
    {
        _connectionString = options.Value.ConnectionString ?? string.Empty;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            return HealthCheckResult.Degraded("Connection string not configured");
        }

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            await using var cmd = new NpgsqlCommand("SELECT 1", connection);
            await cmd.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("Database connection successful");
        }
        catch (NpgsqlException ex) when (ex.SqlState == "08001" || ex.SqlState == "08006")
        {
            return HealthCheckResult.Degraded($"Database unavailable: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Database check failed: {ex.Message}", ex);
        }
    }
}

public sealed class DatabaseHealthCheckOptions
{
    public string ConnectionString { get; set; } = string.Empty;
}