using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Octopus.Server.Extensibility.Extensions;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira
{
    [OctopusPlugin("Jira Issue Tracker", "Octopus Deploy")]
    public class JiraIssueTrackerExtension : IOctopusExtension
    {
        public void Load(ContainerBuilder builder)
        {
            builder.RegisterType<JiraConfigurationMapping>()
                .As<IConfigurationDocumentMapper>()
                .InstancePerDependency();
            
            builder.RegisterType<JiraConfigurationStore>()
                .As<IJiraConfigurationStore>()
                .InstancePerDependency();
        }
    }
}
