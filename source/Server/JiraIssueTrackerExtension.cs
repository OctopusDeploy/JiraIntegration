using System;
using Autofac;
using Octopus.Server.Extensibility.Extensions;
using Octopus.Server.Extensibility.Extensions.Infrastructure;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;
using Octopus.Server.Extensibility.Extensions.Mappings;
using Octopus.Server.Extensibility.IssueTracker.Extensions;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;
using Octopus.Server.Extensibility.IssueTracker.Jira.Deployments;

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

            builder.RegisterType<JiraIssueTracker>()
                .As<IIssueTracker>()
                .InstancePerLifetimeScope();

            builder.RegisterType<DeploymentObserver>().AsSelf().InstancePerDependency();

            builder.RegisterType<JiraConfigureCommands>()
                .As<IContributeToConfigureCommand>()
                .InstancePerDependency();
        }
    }
}
