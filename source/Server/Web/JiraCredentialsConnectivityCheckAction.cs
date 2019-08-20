using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;
using Octopus.Server.Extensibility.IssueTracker.Jira.Integration;
using Octopus.Server.Extensibility.IssueTracker.Jira.Web.Response;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Web
{
    public class JiraCredentialsConnectivityCheckAction : IAsyncApiAction
    {
        private readonly IJiraConfigurationStore configurationStore;
        private readonly ILog log;

        public JiraCredentialsConnectivityCheckAction(IJiraConfigurationStore configurationStore, ILog log)
        {
            this.configurationStore = configurationStore;
            this.log = log;
        }

        public async Task ExecuteAsync(OctoContext context)
        {
            var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<JObject>(json);

            var baseUrl = request.GetValue("BaseUrl").ToString();
            var username = request.GetValue("Username").ToString();
            // If password here is null, it could be that they're clicking the test connectivity button after saving
            // the configuration as we won't have the value of the password on client side, so we need to retrieve it
            // from the database
            var password = string.IsNullOrEmpty(request.GetValue("Password").ToString())
                ? configurationStore.GetJiraPassword()
                : request.GetValue("Password").ToString();
            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                context.Response.AsOctopusJson(ConnectivityCheckResponse.Failure(
                    string.IsNullOrEmpty(baseUrl) ? "Please provide a value for Jira Base Url." : null,
                    string.IsNullOrEmpty(username) ? "Please provide a value for Jira Username." : null,
                    string.IsNullOrEmpty(password) ? "Please provide a value for Jira Password." : null));
                return;
            }

            var jiraRestClient = new JiraRestClient(baseUrl, username, password, log);
            var connectivityCheckResult = jiraRestClient.GetServerInfo().Result;
            context.Response.AsOctopusJson(connectivityCheckResult);
        }
    }
}