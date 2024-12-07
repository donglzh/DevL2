using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DevL2.WebAPI.HealthCheck
{
    public class ThirdPartyServiceHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ThirdPartyServiceHealthCheck(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var serviceConfigs = _configuration.GetSection("ThirdPartyServices")
                .GetChildren()
                .ToList();

            if (serviceConfigs.Count == 0)
            {
                return HealthCheckResult.Unhealthy("No third-party service configuration found.");
            }

            var checkResults = new Dictionary<string, string>();

            foreach (var serviceConfig in serviceConfigs)
            {
                var serviceName = serviceConfig["Name"];
                var serviceUrl = serviceConfig["Url"];

                if (string.IsNullOrWhiteSpace(serviceName) || string.IsNullOrWhiteSpace(serviceUrl))
                {
                    checkResults[serviceName ?? "Unnamed Service"] = "Missing service name or URL.";
                    continue;
                }

                try
                {
                    var status = await CheckServiceAsync(serviceUrl, cancellationToken);
                    checkResults[serviceName] = status ? "Healthy" : "Unhealthy: Service did not respond successfully.";
                }
                catch (Exception ex)
                {
                    checkResults[serviceName] = $"Error: {ex.Message}";
                }
            }

            var data = checkResults.ToDictionary(kv => kv.Key, kv => (object)kv.Value);

            var overallStatus =
                checkResults.Values.All(status => status.Contains("Healthy", StringComparison.OrdinalIgnoreCase))
                    ? HealthCheckResult.Healthy("All third-party services are healthy.", data)
                    : HealthCheckResult.Unhealthy("One or more third-party services are unhealthy.", null, data);

            return overallStatus;
        }

        private async Task<bool> CheckServiceAsync(string serviceUrl, CancellationToken cancellationToken)
        {
            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(serviceUrl, cancellationToken);
            return response.IsSuccessStatusCode;
        }
    }
}
