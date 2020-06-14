using System.Linq;
using System.Threading.Tasks;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Server.Extensibility.Resources.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.Web
{
    class JiraCredentialsConnectivityCheckAction : IAsyncApiAction
    {
        static readonly RequestBodyRegistration<JiraCredentialsConnectionCheckData> Data = new RequestBodyRegistration<JiraCredentialsConnectionCheckData>();
        static readonly OctopusJsonRegistration<ConnectivityCheckResponse> Result = new OctopusJsonRegistration<ConnectivityCheckResponse>();

        private readonly IJiraConfigurationStore configurationStore;
        private readonly IOctopusHttpClientFactory octopusHttpClientFactory;
        private readonly ILog log;

        public JiraCredentialsConnectivityCheckAction(IJiraConfigurationStore configurationStore, IOctopusHttpClientFactory octopusHttpClientFactory, ILog log)
        {
            this.configurationStore = configurationStore;
            this.octopusHttpClientFactory = octopusHttpClientFactory;
            this.log = log;
        }

        public async Task<IOctoResponseProvider> ExecuteAsync(IOctoRequest request)
        {
            var requestData = request.GetBody(Data);

            var baseUrl = requestData.BaseUrl;
            var username = requestData.Username;
            // If password here is null, it could be that they're clicking the test connectivity button after saving
            // the configuration as we won't have the value of the password on client side, so we need to retrieve it
            // from the database
            var password = string.IsNullOrEmpty(requestData.Password)
                ? configurationStore.GetJiraPassword()?.Value
                : requestData.Password;
            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                var response = new ConnectivityCheckResponse();
                if (string.IsNullOrEmpty(baseUrl)) response.AddMessage(ConnectivityCheckMessageCategory.Error, "Please provide a value for Jira Base Url.");
                if (string.IsNullOrEmpty(username)) response.AddMessage(ConnectivityCheckMessageCategory.Error, "Please provide a value for Jira Username.");
                if (string.IsNullOrEmpty(password)) response.AddMessage(ConnectivityCheckMessageCategory.Error, "Please provide a value for Jira Password.");
                return Result.Response(response);
            }

            var jiraRestClient = new JiraRestClient(baseUrl, username, password, log, octopusHttpClientFactory);
            var connectivityCheckResponse = jiraRestClient.ConnectivityCheck().GetAwaiter().GetResult();
            if (connectivityCheckResponse.Messages.All(m => m.Category != ConnectivityCheckMessageCategory.Error))
            {
                connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Info, "The Jira Connect App connection was tested successfully");

                if (!configurationStore.GetIsEnabled())
                {
                    connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Warning, "The Jira Integration is not enabled, so its functionality will not currently be available");
                }
            }

            return Result.Response(connectivityCheckResponse);
        }
    }
    
    class JiraCredentialsConnectionCheckData
    {
        public string BaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

}