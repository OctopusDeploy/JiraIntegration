using System;
using System.Linq;
using Newtonsoft.Json;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    internal class JiraPayloadData
    {
        [JsonProperty("deployments")]
        public JiraDeploymentData[] Deployments { get; set; } = Array.Empty<JiraDeploymentData>();
    }

    internal class JiraDeploymentData
    {
        [JsonProperty("deploymentSequenceNumber")]
        public int DeploymentSequenceNumber { get; set; }

        [JsonProperty("updateSequenceNumber")] public long UpdateSequenceNumber { get; set; }

        [JsonProperty("displayName")] public string DisplayName { get; set; } = string.Empty;

        [JsonProperty("associations")]
        public JiraAssociation[] Associations { get; set; } = Array.Empty<JiraAssociation>();

        [JsonProperty("url")] public string Url { get; set; } = string.Empty;

        [JsonProperty("description")] public string? Description { get; set; }

        [JsonProperty("lastUpdated")] public DateTimeOffset LastUpdated { get; set; }

        [JsonProperty("state")] public string State { get; set; } = string.Empty;

        [JsonProperty("pipeline")] public JiraDeploymentPipeline Pipeline { get; set; } = new();

        [JsonProperty("environment")] public JiraDeploymentEnvironment Environment { get; set; } = new();

        [JsonProperty("id")] public string SchemeVersion { get; set; } = string.Empty;
    }

    internal class JiraDeploymentPipeline
    {
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;

        [JsonProperty("displayName")] public string DisplayName { get; set; } = string.Empty;

        [JsonProperty("url")] public string Url { get; set; } = string.Empty;
    }

    internal class JiraDeploymentEnvironment
    {
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;

        [JsonProperty("displayName")] public string DisplayName { get; set; } = string.Empty;

        [JsonProperty("type")] public string Type { get; set; } = string.Empty;
    }

    internal class JiraAssociation
    {
        private string associationType = string.Empty;

        [JsonProperty("associationType")]
        public string AssociationType
        {
            get => associationType;
            set
            {
                if (!JiraAssociationConstants.ValidJiraAssociationTypes.Contains(value))
                    throw new Exception($"Association Type {value} is not a valid type.");

                associationType = value;
            }
        }

        [JsonProperty("values")] public string[] Values { get; set; } = Array.Empty<string>();
    }
}