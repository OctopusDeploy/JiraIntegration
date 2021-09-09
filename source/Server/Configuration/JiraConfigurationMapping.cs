using System;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.Configuration
{
    internal class JiraConfigurationMapping : IConfigurationDocumentMapper
    {
        public Type GetTypeToMap()
        {
            return typeof(JiraConfiguration);
        }
    }
}