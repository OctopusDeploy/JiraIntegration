using System;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Configuration
{
    class JiraConfigurationMapping : IConfigurationDocumentMapper
    {
        public Type GetTypeToMap() => typeof(JiraConfiguration);
    }
}