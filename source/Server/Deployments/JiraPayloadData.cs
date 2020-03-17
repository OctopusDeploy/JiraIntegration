using System;
using Newtonsoft.Json;

namespace Octopus.Server.Extensibility.JiraIntegration.Deployments
{
    class JiraPayloadData
    {
        [JsonProperty("deployments")]
        public JiraDeploymentData[] Deployments { get; set; }
    }

    class JiraDeploymentData
    {
        [JsonProperty("deploymentSequenceNumber")]
        public int DeploymentSequenceNumber { get; set; }
        [JsonProperty("updateSequenceNumber")]
        public long UpdateSequenceNumber { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("issueKeys")]
        public string[] IssueKeys { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("lastUpdated")]
        public DateTimeOffset LastUpdated { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("pipeline")]
        public JiraDeploymentPipeline Pipeline { get; set; }

        [JsonProperty("environment")]
        public JiraDeploymentEnvironment Environment { get; set; }

        [JsonProperty("id")]
        public string SchemeVersion { get; set; }
    }

    class JiraDeploymentPipeline
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    class JiraDeploymentEnvironment
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }

}