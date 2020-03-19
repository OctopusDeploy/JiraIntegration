using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Domain.Deployments;
using Octopus.Server.Extensibility.Extensions.Domain;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.HostServices.Configuration;
using Octopus.Server.Extensibility.HostServices.Domain.Environments;
using Octopus.Server.Extensibility.HostServices.Domain.Projects;
using Octopus.Server.Extensibility.HostServices.Domain.ServerTasks;
using Octopus.Server.Extensibility.HostServices.Licensing;
using Octopus.Server.Extensibility.HostServices.Model.Environments;
using Octopus.Server.Extensibility.HostServices.Model.Projects;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.JiraIntegration.Environments;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Time;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    class DeploymentObserver : IObserveDomainEvent<DeploymentEvent>
    {
        private readonly ILogWithContext log;
        private readonly IJiraConfigurationStore store;
        private readonly JiraConnectAppClient connectAppClient;
        private readonly IInstallationIdProvider installationIdProvider;
        private readonly IClock clock;
        private readonly IProvideDeploymentEnvironmentSettingsValues deploymentEnvironmentSettingsProvider;
        private readonly IServerConfigurationStore serverConfigurationStore;
        private readonly IProjectStore projectStore;
        private readonly IDeploymentEnvironmentStore deploymentEnvironmentStore;
        private readonly IReleaseStore releaseStore;
        private readonly IServerTaskStore serverTaskStore;
        private readonly IOctopusHttpClientFactory octopusHttpClientFactory;

        public DeploymentObserver(ILogWithContext log,
            IJiraConfigurationStore store,
            JiraConnectAppClient connectAppClient,
            IInstallationIdProvider installationIdProvider,
            IClock clock,
            IProvideDeploymentEnvironmentSettingsValues deploymentEnvironmentSettingsProvider,
            IServerConfigurationStore serverConfigurationStore,
            IProjectStore projectStore,
            IDeploymentEnvironmentStore deploymentEnvironmentStore,
            IReleaseStore releaseStore,
            IServerTaskStore serverTaskStore,
            IOctopusHttpClientFactory octopusHttpClientFactory)
        {
            this.log = log;
            this.store = store;
            this.connectAppClient = connectAppClient;
            this.installationIdProvider = installationIdProvider;
            this.clock = clock;
            this.deploymentEnvironmentSettingsProvider = deploymentEnvironmentSettingsProvider;
            this.serverConfigurationStore = serverConfigurationStore;
            this.projectStore = projectStore;
            this.deploymentEnvironmentStore = deploymentEnvironmentStore;
            this.releaseStore = releaseStore;
            this.serverTaskStore = serverTaskStore;
            this.octopusHttpClientFactory = octopusHttpClientFactory;
        }

        public void Handle(DeploymentEvent domainEvent)
        {
            if (!store.GetIsEnabled() ||
                store.GetJiraInstanceType() == JiraInstanceType.Server ||
                domainEvent.Deployment.Changes.All(drn => drn.VersionBuildInformation.All(pm => pm.WorkItems.All(wi => wi.Source != JiraConfigurationStore.CommentParser))))
                return;

            using (log.OpenBlock($"Sending Jira state update - {StateFromEventType(domainEvent.EventType)}"))
            {
                if (string.IsNullOrWhiteSpace(store.GetConnectAppUrl()) ||
                    string.IsNullOrWhiteSpace(store.GetConnectAppPassword()))
                {
                    log.Warn("Jira integration is enabled but settings are incomplete, ignoring deployment events");
                    log.Finish();
                    return;
                }

                // get token from connect App
                var token = connectAppClient.GetAuthTokenFromConnectApp();
                if (token is null)
                {
                    log.Finish();
                    return;
                }

                // Push data to Jira
                PublishToJira(token, domainEvent.EventType, domainEvent.Deployment);

                log.Finish();
            }
        }

        void PublishToJira(string token, DeploymentEventType eventType, IDeployment deployment)
        {
            var envSettings =
                deploymentEnvironmentSettingsProvider
                    .GetSettings<DeploymentEnvironmentSettingsMetadataProvider.JiraDeploymentEnvironmentSettings>(
                        JiraConfigurationStore.SingletonId, deployment.EnvironmentId) ?? new DeploymentEnvironmentSettingsMetadataProvider.JiraDeploymentEnvironmentSettings();
            var serverUri = serverConfigurationStore.GetServerUri()?.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(serverUri))
            {
                log.Warn("To use Jira integration you must have the Octopus server's external url configured (see the Configuration/Nodes page)");
                return;
            }

            var project = projectStore.Get(deployment.ProjectId);
            var deploymentEnvironment = deploymentEnvironmentStore.Get(deployment.EnvironmentId);
            var release = releaseStore.Get(deployment.ReleaseId);
            var serverTask = serverTaskStore.Get(deployment.TaskId);

            var data = new OctopusJiraPayloadData
            {
                InstallationId = installationIdProvider.GetInstallationId().ToString(),
                BaseHostUrl = store.GetBaseUrl(),
                DeploymentsInfo = new JiraPayloadData
                {
                    Deployments = new[]
                    {
                        new JiraDeploymentData
                        {
                            DeploymentSequenceNumber = int.Parse(deployment.Id.Split('-')[1]),
                            UpdateSequenceNumber = DateTime.UtcNow.Ticks,
                            DisplayName = serverTask.Description,
                            IssueKeys = deployment.Changes.SelectMany(drn => drn.VersionBuildInformation
                                .SelectMany(pm => pm.WorkItems)
                                .Where(wi => wi.Source == JiraConfigurationStore.CommentParser)
                                .Select(wi => wi.Id)
                                .Distinct()).ToArray(),
                            Url =
                                $"{serverUri}/app#/{project.SpaceId}/projects/{project.Slug}/releases/{release.Version}/deployments/{deployment.Id}",
                            Description = serverTask.Description,
                            LastUpdated = clock.GetUtcTime(),
                            State = StateFromEventType(eventType),
                            Pipeline = new JiraDeploymentPipeline
                            {
                                Id = deployment.ProjectId,
                                DisplayName = project.Name,
                                Url = $"{serverUri}/app#/{project.SpaceId}/projects/{project.Slug}"
                            },
                            Environment = new JiraDeploymentEnvironment
                            {
                                Id = $"{deployment.EnvironmentId}{(string.IsNullOrWhiteSpace(deployment.TenantId) ? "" : $"-{deployment.TenantId}")}",
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

            using (var client = octopusHttpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var result = client.PostAsync($"{store.GetConnectAppUrl()}/relay/bulk", httpContent).GetAwaiter().GetResult();

                if (!result.IsSuccessStatusCode)
                    log.ErrorFormat("Unable to publish data to Jira. Response code: {0}, Message: {1}", result.StatusCode, result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
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