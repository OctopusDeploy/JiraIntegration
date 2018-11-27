using System;
using System.Collections.Generic;
using System.Text;
using Octopus.Data.Storage.Configuration;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Configuration
{
    public class JiraConfigurationStore : ExtensionConfigurationStore<JiraConfiguration>
    {
        public static string SingletonId = "issuetracker-jira";
        
        public JiraConfigurationStore(IConfigurationStore configurationStore) : base(configurationStore)
        {
        }

        public override string Id => SingletonId;
    }
}
