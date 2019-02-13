using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Domain.Deployments;
using Octopus.Server.Extensibility.Extensions.Domain;
using Octopus.Server.Extensibility.HostServices.Configuration;
using Octopus.Server.Extensibility.HostServices.Domain.Environments;
using Octopus.Server.Extensibility.HostServices.Domain.Projects;
using Octopus.Server.Extensibility.HostServices.Licensing;
using Octopus.Server.Extensibility.HostServices.Model.Environments;
using Octopus.Server.Extensibility.HostServices.Model.Projects;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;
using Octopus.Server.Extensibility.IssueTracker.Jira.Environments;
using Octopus.Shared;
using Octopus.Time;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Deployments
{
    public class DeploymentObserver : IObserveDomainEventAsync<DeploymentEvent>
    {
        private readonly ILog log;
        private readonly IJiraConfigurationStore store;
        private readonly IInstallationIdProvider installationIdProvider;
        private readonly IClock clock;
        private readonly IProvideDeploymentEnvironmentSettingsValues deploymentEnvironmentSettingsProvider;
        private readonly IServerConfigurationStore serverConfigurationStore;
        private readonly IProjectStore projectStore;
        private readonly IDeploymentEnvironmentStore deploymentEnvironmentStore;
        private readonly IReleaseStore releaseStore;

        public DeploymentObserver(ILog log,
            IJiraConfigurationStore store,
            IInstallationIdProvider installationIdProvider,
            IClock clock,
            IProvideDeploymentEnvironmentSettingsValues deploymentEnvironmentSettingsProvider,
            IServerConfigurationStore serverConfigurationStore,
            IProjectStore projectStore,
            IDeploymentEnvironmentStore deploymentEnvironmentStore,
            IReleaseStore releaseStore)
        {
            this.log = log;
            this.store = store;
            this.installationIdProvider = installationIdProvider;
            this.clock = clock;
            this.deploymentEnvironmentSettingsProvider = deploymentEnvironmentSettingsProvider;
            this.serverConfigurationStore = serverConfigurationStore;
            this.projectStore = projectStore;
            this.deploymentEnvironmentStore = deploymentEnvironmentStore;
            this.releaseStore = releaseStore;
        }

        public async Task HandleAsync(DeploymentEvent domainEvent)
        {
            if (!store.GetIsEnabled())
                return;

            if (string.IsNullOrWhiteSpace(store.GetConnectAppUrl()) || string.IsNullOrWhiteSpace(store.GetPassword()))
            {
                log.Warn("Jira integration is enabled but settings are incomplete, ignoring deployment events");
                return;
            }

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
            var envSettings = deploymentEnvironmentSettingsProvider.GetSettings<DeploymentEnvironmentSettingsMetadataProvider.JiraDeploymentEnvironmentSettings>(JiraConfigurationStore.SingletonId, deployment.EnvironmentId);
            var serverUri = serverConfigurationStore.GetServerUri()?.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(serverUri))
                throw new ControlledFailureException("To use Jira integration you must have the Octopus server's external url configured (see the Configuration/Nodes page)");

            var project = projectStore.Get(deployment.ProjectId);
            var deploymentEnvironment = deploymentEnvironmentStore.Get(deployment.EnvironmentId);
            var release = releaseStore.Get(deployment.ReleaseId);

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
                                DeploymentSequenceNumber = int.Parse(deployment.Id.Split('-')[1]),
                                UpdateSequenceNumber = DateTime.UtcNow.Ticks,
                                DisplayName = deployment.Name,
                                IssueKeys = deployment.WorkItems.Where(wi => wi.IssueTrackerId == JiraConfigurationStore.SingletonId).Select(wi => wi.Id).ToArray(),
                                Url = $"{serverUri}/app#/{project.SpaceId}/projects/{project.Slug}/releases/{release.Version}/deployments/{deployment.Id}",
                                Description = deployment.Name,
                                LastUpdated = clock.GetUtcTime(),
                                State = StateFromEventType(eventType),
                                Pipeline = new JiraPayloadData.JiraDeploymentPipeline
                                {
                                    Id = deployment.ProjectId,
                                    DisplayName = project.Name,
                                    Url = $"{serverUri}/app#/{project.SpaceId}/projects/{project.Slug}"
                                },
                                Environment = new JiraPayloadData.JiraDeploymentEnvironment
                                {
                                    Id = deployment.EnvironmentId,
                                    DisplayName = deploymentEnvironment.Name,
                                    Type = envSettings.JiraEnvironmentType.ToString()
                                },
                                SchemeVersion = "1.0"
                            }
                        }
                    }
            };

            log.Info($"Sending deployment data to Jira for deployment {deployment.Id}, to {deploymentEnvironment.Name}({envSettings.JiraEnvironmentType.ToString()}) with state {data.DeploymentsInfo.Deployments[0].State} for issue keys {string.Join(",", data.DeploymentsInfo.Deployments[0].IssueKeys)}");

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
                    return "in_progress";
                case DeploymentEventType.DeploymentFailed:
                    return "failed";
                case DeploymentEventType.DeploymentSucceeded:
                    return "successful";
                default:
                    return "unknown";
            }
        }
    }
}