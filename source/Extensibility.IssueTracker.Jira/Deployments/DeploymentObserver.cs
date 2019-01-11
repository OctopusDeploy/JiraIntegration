using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octopus.Server.Extensibility.Extensions.Domain;
using Octopus.Server.Extensibility.HostServices.Licensing;
using Octopus.Server.Extensibility.HostServices.Model.Projects;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;
using Octopus.Shared;
using Octopus.Time;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Deployments
{
    public class DeploymentObserver : IObserveDomainEventAsync<DeploymentEvent>
    {
        private readonly IJiraConfigurationStore store;
        private readonly IInstallationIdProvider installationIdProvider;
        private readonly IClock clock;

        public DeploymentObserver(IJiraConfigurationStore store,
            IInstallationIdProvider installationIdProvider,
            IClock clock)
        {
            this.store = store;
            this.installationIdProvider = installationIdProvider;
            this.clock = clock;
        }

        public async Task HandleAsync(DeploymentEvent domainEvent)
        {
            if (!store.GetIsEnabled())
                return;

            // get token from connect App
            var token = await GetAuthTokenFromConnectApp();

            // Push data to Jira
            await PublishToJira(token, domainEvent.EventType, domainEvent.Deployment);
        }

        async Task<string> GetAuthTokenFromConnectApp()
        {
            using (var client = new HttpClient())
            {
                var username = installationIdProvider.GetInstallationId();
                var password = store.GetPassword();
                var encodedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedAuth);
                var result = await client.GetAsync($"{store.GetConnectAppUrl()}/token");

                if (result.IsSuccessStatusCode)
                {
                    var authTokenFromConnectApp = JsonConvert.DeserializeObject<JsonTokenData>(await result.Content.ReadAsStringAsync());
                    return authTokenFromConnectApp.Token;
                }

                throw new ControlledFailureException($"Unable to get authentication token for Jira Connect App. Response code: {result.StatusCode}");
            }
        }

        class JsonTokenData
        {
            [JsonProperty("token")]
            public string Token { get; set; }
        }

        async Task PublishToJira(string token, DeploymentEventType eventType, IDeployment deployment)
        {
            var data = new OctopusJiraPayloadData
            {
                InstallationId = installationIdProvider.GetInstallationId().ToString(),
                BaseHostUrl = store.GetBaseUrl(),
                DeploymentsInfo =new JiraPayloadData
                    {
                        Deployments = new[]
                        {
                            new JiraPayloadData.JiraDeploymentData
                            {
                                DeploymentSequenceNumber = 1,
                                UpdateSequenceNumber = 1,
                                DisplayName = deployment.Name,
                                IssueKeys = deployment.WorkItems.Where(wi => wi.IssueTrackerId == JiraConfigurationStore.SingletonId).Select(wi => wi.Id).ToArray(),
                                Url = "https://thisOctopusServerUrl/deployments",
                                Description = deployment.Name,
                                LastUpdated = clock.GetUtcTime(),
                                State = StateFromEventType(eventType),
                                Pipeline = new JiraPayloadData.JiraDeploymentPipeline
                                {
                                    Id = deployment.ProjectId,
                                    DisplayName = "Project's name goes here",
                                    Url = "https://projectUrl"
                                },
                                Environment = new JiraPayloadData.JiraDeploymentEnvironment
                                {
                                    Id = deployment.EnvironmentId,
                                    DisplayName = "Dev",
                                    Type = "DEVELOPMENT"
                                },
                                SchemeVersion = "1.0"
                            }
                        }
                    }
            };

            var json = JsonConvert.SerializeObject(data);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var result = await client.PostAsync($"{store.GetConnectAppUrl()}/relay/bulk", httpContent);

                if (!result.IsSuccessStatusCode)
                    throw new ControlledFailureException($"Unable to publish data to Jira. Response code: {result.StatusCode}");
            }
        }

        string StateFromEventType(DeploymentEventType eventType)
        {
            switch (eventType)
            {
                case DeploymentEventType.DeploymentStarted:
                    return "IN_PROGRESS";
                case DeploymentEventType.DeploymentFailed:
                    return "FAILED";
                case DeploymentEventType.DeploymentSucceeded:
                    return "SUCCESSFUL";
                default:
                    return "UNKNOWN";
            }
        }
    }
}