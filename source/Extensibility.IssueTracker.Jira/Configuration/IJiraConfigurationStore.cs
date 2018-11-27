using System;
using System.Collections.Generic;
using System.Text;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Configuration
{
    public interface IJiraConfigurationStore : IExtensionConfigurationStore<JiraConfiguration>
    {
    }
}
