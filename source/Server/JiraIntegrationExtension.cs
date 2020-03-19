using Autofac;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions;
using Octopus.Server.Extensibility.Extensions.Infrastructure;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.Extensions.Mappings;
using Octopus.Server.Extensibility.Extensions.Model.Environments;
using Octopus.Server.Extensibility.Extensions.WorkItems;
using Octopus.Server.Extensibility.HostServices.Web;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.JiraIntegration.Deployments;
using Octopus.Server.Extensibility.JiraIntegration.Environments;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Server.Extensibility.JiraIntegration.Web;
using Octopus.Server.Extensibility.JiraIntegration.WorkItems;

namespace Octopus.Server.Extensibility.JiraIntegration
{
    [OctopusPlugin("Jira Integration", "Octopus Deploy")]
    public class JiraIntegrationExtension : IOctopusExtension
    {
        public void Load(ContainerBuilder builder)
        {
            builder.RegisterType<JiraConfigurationMapping>()
                .As<IConfigurationDocumentMapper>()
                .InstancePerLifetimeScope();
            
            builder.RegisterType<DatabaseInitializer>().As<IExecuteWhenDatabaseInitializes>().InstancePerLifetimeScope();

            builder.RegisterType<JiraConfigurationStore>()
                .As<IJiraConfigurationStore>()
                .InstancePerLifetimeScope();

            builder.RegisterType<JiraConfigurationSettings>()
                .As<IJiraConfigurationSettings>()
                .As<IHasConfigurationSettings>()
                .As<IHasConfigurationSettingsResource>()
                .As<IContributeMappings>()
                .InstancePerLifetimeScope();

            builder.RegisterType<JiraConnectAppClient>().AsSelf().InstancePerDependency();
            
            builder.RegisterType<JiraIntegration>()
                .As<IIssueTracker>()
                .InstancePerLifetimeScope();

            builder.RegisterType<DeploymentObserver>().AsImplementedInterfaces().InstancePerDependency();

            builder.RegisterType<DeploymentEnvironmentSettingsMetadataProvider>().As<IContributeDeploymentEnvironmentSettingsMetadata>().InstancePerDependency();

            builder.RegisterType<JiraConfigureCommands>()
                .As<IContributeToConfigureCommand>()
                .InstancePerDependency();

            builder.RegisterType<JiraConnectAppConnectivityCheckAction>().AsSelf().InstancePerDependency();
            builder.RegisterType<JiraCredentialsConnectivityCheckAction>().AsSelf().InstancePerDependency();
            
            builder.RegisterType<CommentParser>().AsSelf().InstancePerDependency();
            builder.RegisterType<WorkItemLinkMapper>().As<IWorkItemLinkMapper>().InstancePerDependency();

            builder.Register(c =>
            {
                var store = c.Resolve<IJiraConfigurationStore>();
                if (!store.GetIsEnabled())
                    return null;
                
                var baseUrl = store.GetBaseUrl();
                var username = store.GetJiraUsername();
                var password = store.GetJiraPassword();
                return new JiraRestClient(
                    baseUrl, 
                    username, 
                    password, 
                    c.Resolve<ILog>(), 
                    c.Resolve<IOctopusHttpClientFactory>()
                );
            }).As<IJiraRestClient>()
            .InstancePerDependency();

            builder.RegisterType<JiraIntegrationHomeLinksContributor>().As<IHomeLinksContributor>()
                .InstancePerDependency();
        }
    }
}
