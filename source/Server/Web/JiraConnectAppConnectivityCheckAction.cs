using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.HostServices.Licensing;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Server.Extensibility.Resources.Configuration;
using Octopus.Server.Extensibility.Web.Extensions;

namespace Octopus.Server.Extensibility.JiraIntegration.Web
{
    [ApiController]
    class JiraConnectAppConnectivityCheckAction : SystemScopedApiController
    {
        private readonly ISystemLog systemLog;
        private readonly IJiraConfigurationStore configurationStore;
        private readonly IInstallationIdProvider installationIdProvider;
        private readonly JiraConnectAppClient connectAppClient;
        private readonly IOctopusHttpClientFactory octopusHttpClientFactory;

        public JiraConnectAppConnectivityCheckAction(
            ISystemLog systemLog,
            IJiraConfigurationStore configurationStore,
            IInstallationIdProvider installationIdProvider,
            JiraConnectAppClient connectAppClient,
            IOctopusHttpClientFactory octopusHttpClientFactory)
        {
            this.systemLog = systemLog;
            this.configurationStore = configurationStore;
            this.installationIdProvider = installationIdProvider;
            this.connectAppClient = connectAppClient;
            this.octopusHttpClientFactory = octopusHttpClientFactory;
        }

        [HttpPost(JiraIntegrationApi.ApiConnectAppCredentialsTest)]
        public async Task<ConnectivityCheckResponse> Execute(JiraConnectAppConnectionCheckData command, CancellationToken cancellationToken)
        {
            var baseUrl = command.BaseUrl;
            var connectivityCheckResponse = new ConnectivityCheckResponse();

            var username = (await installationIdProvider.GetInstallationIdAsync(cancellationToken)).ToString();

            // If password here is null, it could be that they're clicking the test connectivity button after saving
            // the configuration as we won't have the value of the password on client side, so we need to retrieve it
            // from the database
            var password = string.IsNullOrEmpty(command.Password) ? (await configurationStore.GetConnectAppPassword(cancellationToken))?.Value : command.Password;
            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(password))
            {
                if (string.IsNullOrEmpty(baseUrl)) connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Error, "Please provide a value for Jira Base Url.");
                if (string.IsNullOrEmpty(password)) connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Error, "Please provide a value for Jira Connect App Password.");
                return connectivityCheckResponse;
            }

            var token = await connectAppClient.GetAuthTokenFromConnectApp(username, password, systemLog, cancellationToken);
            if (token is null)
            {
                connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Error, "Failed to get authentication token from Jira Connect App.");
                return connectivityCheckResponse;
            }

            using (var client = octopusHttpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var connectivityCheckPayload =
                    JsonConvert.SerializeObject(new JiraConnectAppConnectivityCheckRequest {BaseHostUrl = baseUrl, OctopusInstallationId = username});
                var result = await client.PostAsync(
                        $"{configurationStore.GetConnectAppUrl(cancellationToken)}/relay/connectivitycheck",
                        new StringContent(connectivityCheckPayload, Encoding.UTF8, "application/json"), cancellationToken);

                if (!result.IsSuccessStatusCode)
                {
                    connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Error, result.StatusCode == HttpStatusCode.NotFound
                        ? $"Failed to find an installation for Jira host {configurationStore.GetBaseUrl(cancellationToken)}. Please ensure you have installed the Octopus Deploy for Jira plugin from the [Atlassian Marketplace](https://marketplace.atlassian.com/apps/1220376/octopus-deploy-for-jira). [Learn more](https://g.octopushq.com/JiraIntegration)."
                        : $"Failed to check connectivity to Jira. Response code: {result.StatusCode}, Message: {await result.Content.ReadAsStringAsync(cancellationToken)}");
                    return connectivityCheckResponse;
                }
            }

            connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Info, "The Jira Connect App connection was tested successfully");

            if (!await configurationStore.GetIsEnabled(cancellationToken))
            {
                connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Warning, "The Jira Integration is not enabled, so its functionality will not currently be available");
            }

            return connectivityCheckResponse;
        }

#nullable disable
        private class JiraConnectAppConnectivityCheckRequest
        {
            public string BaseHostUrl { get; set; } = string.Empty;
            public string OctopusInstallationId { get; set; } = string.Empty;
        }
    }

    class JiraConnectAppConnectionCheckData
    {
        public string BaseUrl { get; set; }
        public string Password { get; set; }
    }
}
