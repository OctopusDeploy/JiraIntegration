using System;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.HostServices.Licensing;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.Integration
{
    class JiraConnectAppClient
    {
        private readonly ILogWithContext log;
        private readonly IInstallationIdProvider installationIdProvider;
        private readonly IJiraConfigurationStore configurationStore;
        private readonly IOctopusHttpClientFactory octopusHttpClientFactory;

        public JiraConnectAppClient(
            ILogWithContext log,
            IInstallationIdProvider installationIdProvider,
            IJiraConfigurationStore configurationStore,
            IOctopusHttpClientFactory octopusHttpClientFactory)
        {
            this.log = log;
            this.installationIdProvider = installationIdProvider;
            this.configurationStore = configurationStore;
            this.octopusHttpClientFactory = octopusHttpClientFactory;
        }

        public string GetAuthTokenFromConnectApp()
        {
            var username = installationIdProvider.GetInstallationId().ToString();
            var password = configurationStore.GetConnectAppPassword();
            return GetAuthTokenFromConnectApp(username, password);
        }
        
        public string GetAuthTokenFromConnectApp(string username, string password)
        {
            using (var client = octopusHttpClientFactory.CreateClient())
            {
                var encodedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedAuth);
                var result = client.GetAsync($"{configurationStore.GetConnectAppUrl()}/token").GetAwaiter().GetResult();

                if (result.IsSuccessStatusCode)
                {
                    var authTokenFromConnectApp = JsonConvert.DeserializeObject<JsonTokenData>(result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    return authTokenFromConnectApp.Token;
                }

                log.ErrorFormat("Unable to get authentication token for Jira Connect App. Response code: {0}", result.StatusCode);
                return null;
            }
        }

        class JsonTokenData
        {
            [JsonProperty("token")]
            public string Token { get; set; }
        }

    }
}