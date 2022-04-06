using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.HostServices.Licensing;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.Integration
{
    internal class JiraConnectAppClient
    {
        private readonly IJiraConfigurationStore configurationStore;
        private readonly IInstallationIdProvider installationIdProvider;
        private readonly IOctopusHttpClientFactory octopusHttpClientFactory;

        public JiraConnectAppClient(
            IInstallationIdProvider installationIdProvider,
            IJiraConfigurationStore configurationStore,
            IOctopusHttpClientFactory octopusHttpClientFactory)
        {
            this.installationIdProvider = installationIdProvider;
            this.configurationStore = configurationStore;
            this.octopusHttpClientFactory = octopusHttpClientFactory;
        }

        public async Task<string?> GetAuthTokenFromConnectApp(ILog log)
        {
            var username = installationIdProvider.GetInstallationId().ToString();
            var password = configurationStore.GetConnectAppPassword();
            return await GetAuthTokenFromConnectApp(username, password?.Value, log);
        }

        public async Task<string?> GetAuthTokenFromConnectApp(string username, string? password, ILog log)
        {
            using var client = octopusHttpClientFactory.CreateClient();
            var encodedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedAuth);
            try
            {
                using var result = await client.GetAsync($"{configurationStore.GetConnectAppUrl()}/token");
                if (result.IsSuccessStatusCode)
                {
                    var authTokenFromConnectApp =
                        JsonConvert.DeserializeObject<JsonTokenData>(result.Content.ReadAsStringAsync().GetAwaiter()
                            .GetResult());
                    return authTokenFromConnectApp.Token;
                }

                log.ErrorFormat("Unable to get authentication token for Jira Connect App. Response code: {0}",
                    result.StatusCode);

                return null;
            }
            catch (HttpRequestException e)
            {
                log.ErrorFormat("Unable to get authentication token for Jira Connect App. Reason: {0}", e.Message);
                return null;
            }
            catch (TaskCanceledException e)
            {
                log.ErrorFormat("Unable to get authentication token for Jira Connect App. Reason: {0}", e.Message);
                return null;
            }
        }

        private class JsonTokenData
        {
            [JsonProperty("token")] public string Token { get; set; } = string.Empty;
        }
    }
}