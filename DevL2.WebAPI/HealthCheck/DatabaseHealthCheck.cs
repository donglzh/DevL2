using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace DevL2.WebAPI.HealthCheck
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;

        public DatabaseHealthCheck(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var dbConnections = _configuration.GetSection("DatabaseConnections")
                .GetChildren()
                .ToList();

            if (dbConnections.Count == 0)
            {
                return HealthCheckResult.Unhealthy("No database configuration found.");
            }

            var checkResults = new Dictionary<string, Dictionary<string, string>>();

            foreach (var dbConfig in dbConnections)
            {
                var dbType = dbConfig["DatabaseType"];
                var connectionString = dbConfig["ConnectionString"];
                var dbName = dbConfig["Name"];

                try
                {
                    HealthCheckResult result = dbType?.ToLower() switch
                    {
                        "sqlserver" => await CheckSqlServerAsync(connectionString, dbName, cancellationToken),
                        "mysql" => await CheckMySqlAsync(connectionString, dbName, cancellationToken),
                        _ => HealthCheckResult.Unhealthy($"Unsupported database type: {dbType}")
                    };

                    if (!checkResults.ContainsKey(dbType))
                    {
                        checkResults[dbType] = new Dictionary<string, string>();
                    }

                    checkResults[dbType][dbName] = result.Description;
                }
                catch (Exception ex)
                {
                    if (!checkResults.ContainsKey(dbType))
                    {
                        checkResults[dbType] = new Dictionary<string, string>();
                    }

                    checkResults[dbType][dbName] = $"Error: {ex.Message}";
                }
            }

            var data = checkResults.ToDictionary(kv => kv.Key, kv => (object)kv.Value);

            var overallStatus = checkResults.Values.SelectMany(r => r.Values)
                .All(status => status.Contains("healthy", StringComparison.OrdinalIgnoreCase))
                ? HealthCheckResult.Healthy("All databases are healthy.", data)
                : HealthCheckResult.Unhealthy("One or more databases are unhealthy.", null, data);

            return overallStatus;
        }

        private async Task<HealthCheckResult> CheckSqlServerAsync(string connectionString, string dbName,
            CancellationToken cancellationToken)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                    return HealthCheckResult.Healthy($"SQL Server database '{dbName}' is healthy.");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"SQL Server database '{dbName}' connection failed: {ex.Message}");
            }
        }

        private async Task<HealthCheckResult> CheckMySqlAsync(string connectionString, string dbName,
            CancellationToken cancellationToken)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                    return HealthCheckResult.Healthy($"MySQL database '{dbName}' is healthy.");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"MySQL database '{dbName}' connection failed: {ex.Message}");
            }
        }
    }
}
