using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Server.Extensibility.Resources.Configuration;
using Octopus.Server.Extensibility.Web.Extensions;

namespace Octopus.Server.Extensibility.JiraIntegration.Web
{
    [ApiController]
    class JiraCredentialsConnectivityCheckAction : SystemScopedApiController
    {
        private readonly IJiraConfigurationStore configurationStore;
        private readonly IOctopusHttpClientFactory octopusHttpClientFactory;
        private readonly ISystemLog systemLog;

        public JiraCredentialsConnectivityCheckAction(IJiraConfigurationStore configurationStore, IOctopusHttpClientFactory octopusHttpClientFactory, ISystemLog systemLog)
        {
            this.configurationStore = configurationStore;
            this.octopusHttpClientFactory = octopusHttpClientFactory;
            this.systemLog = systemLog;
        }

        [HttpPost(JiraIntegrationApi.ApiJiraCredentialsTest)]
        public async Task<ConnectivityCheckResponse> Execute(JiraCredentialsConnectionCheckData command, CancellationToken cancellationToken)
        {
            var baseUrl = command.BaseUrl;
            var username = command.Username;
            // If password here is null, it could be that they're clicking the test connectivity button after saving
            // the configuration as we won't have the value of the password on client side, so we need to retrieve it
            // from the database
            var password = string.IsNullOrEmpty(command.Password)
                ? (await configurationStore.GetJiraPassword(cancellationToken))?.Value
                : command.Password;
            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                var response = new ConnectivityCheckResponse();
                if (string.IsNullOrEmpty(baseUrl)) response.AddMessage(ConnectivityCheckMessageCategory.Error, "Please provide a value for Jira Base Url.");
                if (string.IsNullOrEmpty(username)) response.AddMessage(ConnectivityCheckMessageCategory.Error, "Please provide a value for Jira Username.");
                if (string.IsNullOrEmpty(password)) response.AddMessage(ConnectivityCheckMessageCategory.Error, "Please provide a value for Jira Password.");
                return response;
            }

            await using var jiraRestClient = new JiraRestClient(baseUrl, username, password, systemLog, octopusHttpClientFactory);
            var connectivityCheckResponse = await jiraRestClient.ConnectivityCheck(cancellationToken);
            if (connectivityCheckResponse.Messages.All(m => m.Category != ConnectivityCheckMessageCategory.Error))
            {
                connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Info, "The Jira Connect App connection was tested successfully");

                if (!await configurationStore.GetIsEnabled(cancellationToken))
                {
                    connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Warning, "The Jira Integration is not enabled, so its functionality will not currently be available");
                }
            }

            return connectivityCheckResponse;
        }
    }

#nullable disable
    class JiraCredentialsConnectionCheckData
    {
        public string BaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
