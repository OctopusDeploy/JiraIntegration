using System;
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
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;
using Octopus.Server.Extensibility.IssueTracker.Jira.Deployments;
using Octopus.Server.Extensibility.IssueTracker.Jira.Environments;
using Octopus.Server.Extensibility.IssueTracker.Jira.Integration;
using Octopus.Server.Extensibility.IssueTracker.Jira.Web;
using Octopus.Server.Extensibility.IssueTracker.Jira.WorkItems;

namespace Octopus.Server.Extensibility.IssueTracker.Jira
{
    [OctopusPlugin("Jira Issue Tracker", "Octopus Deploy")]
    public class JiraIssueTrackerExtension : IOctopusExtension
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
            
            builder.RegisterType<JiraIssueTracker>()
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

            builder.RegisterType<JiraIssueTrackerHomeLinksContributor>().As<IHomeLinksContributor>()
                .InstancePerDependency();
        }
    }
}
