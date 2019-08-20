using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.HostServices.Licensing;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;
using Octopus.Server.Extensibility.IssueTracker.Jira.Integration;
using Octopus.Server.Extensibility.IssueTracker.Jira.Web.Response;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Web
{
    public class JiraConnectAppConnectivityCheckAction : IAsyncApiAction
    {
        private readonly IJiraConfigurationStore configurationStore;
        private readonly IInstallationIdProvider installationIdProvider;
        private readonly JiraConnectAppClient connectAppClient;

        public JiraConnectAppConnectivityCheckAction(
            IJiraConfigurationStore configurationStore, 
            IInstallationIdProvider installationIdProvider,
            JiraConnectAppClient connectAppClient)
        {
            this.configurationStore = configurationStore;
            this.installationIdProvider = installationIdProvider;
            this.connectAppClient = connectAppClient;
        }
        
        public async Task ExecuteAsync(OctoContext context)
        {
            var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<JObject>(json);
            var baseUrl = request.GetValue("BaseUrl").ToString();
            
            var username = installationIdProvider.GetInstallationId().ToString();
            // If password here is null, it could be that they're clicking the test connectivity button after saving
            // the configuration as we won't have the value of the password on client side, so we need to retrieve it
            // from the database
            var password = string.IsNullOrEmpty(request.GetValue("Password").ToString()) ? configurationStore.GetConnectAppPassword() : request.GetValue("Password").ToString();
            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(password))
            {
                context.Response.AsOctopusJson(ConnectivityCheckResponse.Failure(
                    string.IsNullOrEmpty(baseUrl) ? "Please provide a value for Jira Base Url." : null,
                    string.IsNullOrEmpty(password) ? "Please provide a value for Jira Connect App Password." : null)
                );
                return;
            }

            var token = connectAppClient.GetAuthTokenFromConnectApp(username, password);
            if (token is null)
            {
                context.Response.AsOctopusJson(ConnectivityCheckResponse.Failure("Failed to get authentication token from Jira Connect App."));
                return;
            }
            
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var connectivityCheckPayload =
                    JsonConvert.SerializeObject(new JiraConnectAppConnectivityCheckRequest {BaseHostUrl = baseUrl, OctopusInstallationId = username});
                var result = await client.PostAsync(
                        $"{configurationStore.GetConnectAppUrl()}/relay/connectivitycheck",
                        new StringContent(connectivityCheckPayload, Encoding.UTF8, "application/json"));

                if (!result.IsSuccessStatusCode)
                {
                    context.Response.AsOctopusJson(ConnectivityCheckResponse.Failure(result.StatusCode == HttpStatusCode.NotFound
                        ? $"Failed to find an installation for Jira host {configurationStore.GetBaseUrl()}. Please ensure you have installed the Octopus Deploy for Jira plugin from the [Atlassian Marketplace](https://marketplace.atlassian.com/apps/1220376/octopus-deploy-for-jira). [Learn more](https://g.octopushq.com/JiraIssueTracker)."
                        : $"Failed to check connectivity to Jira. Response code: {result.StatusCode}, Message: {result.Content.ReadAsStringAsync().GetAwaiter().GetResult()}")
                    );
                    return;
                }

                context.Response.AsOctopusJson(ConnectivityCheckResponse.Success);
            }
        }

        private class JiraConnectAppConnectivityCheckRequest
        {
            public string BaseHostUrl { get; set; }
            public string OctopusInstallationId { get; set; }
        }
    }
}