using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.HostServices.Configuration;
using Octopus.Server.Extensibility.HostServices.Diagnostics;
using Octopus.Server.Extensibility.HostServices.Licensing;
using Octopus.Server.Extensibility.HostServices.Model.Environments;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.JiraIntegration.Environments;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Server.Extensibility.Mediator;
using Octopus.Server.MessageContracts.Features.DeploymentEnvironments;
using Octopus.Server.MessageContracts.Features.Projects;
using Octopus.Server.MessageContracts.Features.Projects.Releases;
using Octopus.Server.MessageContracts.Features.Projects.Releases.Deployments;
using Octopus.Server.MessageContracts.Features.ServerTasks;
using Octopus.Time;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    class JiraDeployment
    {
        private readonly IMediator mediator;
        private readonly ITaskLogFactory taskLogFactory;
        private readonly IJiraConfigurationStore store;
        private readonly JiraConnectAppClient connectAppClient;
        private readonly IInstallationIdProvider installationIdProvider;
        private readonly IClock clock;
        private readonly IProvideDeploymentEnvironmentSettingsValues deploymentEnvironmentSettingsProvider;
        private readonly IServerConfigurationStore serverConfigurationStore;
        private readonly IOctopusHttpClientFactory octopusHttpClientFactory;

        private DeploymentEnvironmentSettingsMetadataProvider.JiraDeploymentEnvironmentSettings? environmentSettings;
        private EnvironmentResource? deploymentEnvironment;

        public JiraDeployment(
            IMediator mediator,
            ITaskLogFactory taskLogFactory,
            IJiraConfigurationStore store,
            JiraConnectAppClient connectAppClient,
            IInstallationIdProvider installationIdProvider,
            IClock clock,
            IProvideDeploymentEnvironmentSettingsValues deploymentEnvironmentSettingsProvider,
            IServerConfigurationStore serverConfigurationStore,
            IOctopusHttpClientFactory octopusHttpClientFactory
        )
        {
            this.mediator = mediator;
            this.taskLogFactory = taskLogFactory;
            this.store = store;
            this.connectAppClient = connectAppClient;
            this.installationIdProvider = installationIdProvider;
            this.clock = clock;
            this.deploymentEnvironmentSettingsProvider = deploymentEnvironmentSettingsProvider;
            this.serverConfigurationStore = serverConfigurationStore;
            this.octopusHttpClientFactory = octopusHttpClientFactory;
        }

        bool JiraIntegrationUnavailable(DeploymentResource deployment)
        {
            return !store.GetIsEnabled() ||
                   store.GetJiraInstanceType() == JiraInstanceType.Server;
        }

        public async Task PublishToJira(string eventType, DeploymentResource deployment, IJiraApiDeployment jiraApiDeployment,
            ITaskLog taskLog, CancellationToken cancellationToken)
        {
            if (JiraIntegrationUnavailable(deployment))
            {
                jiraApiDeployment.HandleJiraIntegrationIsUnavailable();
                return;
            }

            var serverUri = serverConfigurationStore.GetServerUri()?.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(serverUri))
            {
                taskLog.Warn("To use Jira integration you must have the Octopus server's external url configured (see the Configuration/Nodes page)");
                return;
            }

            if (string.IsNullOrWhiteSpace(store.GetConnectAppUrl()) ||
                string.IsNullOrWhiteSpace(store.GetConnectAppPassword()?.Value))
            {
                taskLog.Warn("Jira integration is enabled but settings are incomplete, ignoring deployment events");
                return;
            }

            var taskLogBlock = taskLogFactory.CreateBlock(taskLog, $"Sending Jira state update - {eventType}");

            // get token from connect App
            var token = connectAppClient.GetAuthTokenFromConnectApp(taskLogBlock);
            if (token is null)
            {
                taskLogFactory.Finish(taskLogBlock);
                return;
            }

            var getDeploymentEnvironmentResponse = await mediator.Request(new GetDeploymentEnvironmentRequest(deployment.EnvironmentId), cancellationToken);
            deploymentEnvironment = getDeploymentEnvironmentResponse.DeploymentEnvironment;
            environmentSettings =
                deploymentEnvironmentSettingsProvider
                    .GetSettings<DeploymentEnvironmentSettingsMetadataProvider.JiraDeploymentEnvironmentSettings>(
                        JiraConfigurationStore.SingletonId, deployment.EnvironmentId.Value) ?? new DeploymentEnvironmentSettingsMetadataProvider.JiraDeploymentEnvironmentSettings();

            var data = await PrepareOctopusJiraPayload(eventType, serverUri, deployment, jiraApiDeployment, cancellationToken);

            // Push data to Jira
            await SendToJira(token, data, deployment, taskLogBlock);

            taskLogFactory.Finish(taskLogBlock);
        }

        async Task<OctopusJiraPayloadData> PrepareOctopusJiraPayload(string eventType, string serverUri, DeploymentResource deployment, IJiraApiDeployment jiraApiDeployment, CancellationToken cancellationToken)
        {
            var project = (await mediator.Request(new GetProjectRequest(deployment.ProjectId), cancellationToken)).Project;

            var release = (await mediator.Request(new GetReleaseRequest(deployment.ReleaseId), cancellationToken)).Release;
            var serverTask = (await mediator.Request(new GetServerTaskRequest(deployment.TaskId), cancellationToken)).Task;

            return new OctopusJiraPayloadData
            {
                InstallationId = installationIdProvider.GetInstallationId().ToString(),
                BaseHostUrl = store.GetBaseUrl() ?? string.Empty,
                DeploymentsInfo = new JiraPayloadData
                {
                    Deployments = new[]
                    {
                        new JiraDeploymentData
                        {
                            DeploymentSequenceNumber = int.Parse(deployment.Id!.ToString().Split('-')[1]),
                            UpdateSequenceNumber = DateTime.UtcNow.Ticks,
                            DisplayName = serverTask.Description,
                            Associations = new []
                            {
                                new JiraAssociation()
                                {
                                    AssociationType = jiraApiDeployment.DeploymentType,
                                    Values = jiraApiDeployment.DeploymentValues(deployment)
                                }
                            },
                            Url =
                                $"{serverUri}/app#/{project.SpaceId}/projects/{project.Slug}/releases/{release.Version}/deployments/{deployment.Id}",
                            Description = serverTask.Description,
                            LastUpdated = clock.GetUtcTime(),
                            State = eventType,
                            Pipeline = new JiraDeploymentPipeline
                            {
                                Id = deployment.ProjectId.Value,
                                DisplayName = project.Name,
                                Url = $"{serverUri}/app#/{project.SpaceId}/projects/{project.Slug}"
                            },
                            Environment = new JiraDeploymentEnvironment
                            {
                                Id = $"{deployment.EnvironmentId}{(deployment.TenantId is null ? "" : $"-{deployment.TenantId}")}",
                                DisplayName = deploymentEnvironment?.Name ?? string.Empty,
                                Type = environmentSettings?.JiraEnvironmentType.ToString() ?? JiraEnvironmentType.unmapped.ToString()
                            },
                            SchemeVersion = "1.0"
                        }
                    }
                }
            };
        }

        async Task SendToJira(string token, OctopusJiraPayloadData data, DeploymentResource deployment, ITaskLog taskLogBlock)
        {
            taskLogBlock.Info($"Sending deployment data to Jira for deployment {deployment.Id}, to {deploymentEnvironment?.Name}({environmentSettings?.JiraEnvironmentType.ToString()}) with state {data.DeploymentsInfo.Deployments[0].State} for issue keys {string.Join(",", data.DeploymentsInfo.Deployments[0].Associations[0].Values)}");

            var json = JsonConvert.SerializeObject(data);

            using (var client = octopusHttpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var result = await client.PostAsync($"{store.GetConnectAppUrl()}/relay/bulk", httpContent);

                if (!result.IsSuccessStatusCode)
                {
                    var errorContent = await result.Content.ReadAsStringAsync();
                    taskLogBlock.ErrorFormat("Unable to publish data to Jira. Response code: {0}, Message: {1}", result.StatusCode, errorContent);
                }
            }
        }
    }
}