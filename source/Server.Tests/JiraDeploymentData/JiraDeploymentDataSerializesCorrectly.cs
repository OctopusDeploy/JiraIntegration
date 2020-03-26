using System;
using Assent;
using Newtonsoft.Json;
using NUnit.Framework;
using Octopus.Server.Extensibility.JiraIntegration.Deployments;
using Octopus.Server.Extensibility.JiraIntegration.Environments;

namespace Octopus.Server.Extensibility.JiraIntegration.Tests.JiraDeploymentData
{
    [TestFixture]
    public class JiraDeploymentDataSerializesCorrectly
    {
        [Test]
        public void DataSerializesCorrectly()
        {
            var data = new OctopusJiraPayloadData
            {
                InstallationId = "12345",
                BaseHostUrl = "https://octopussample.com",
                DeploymentsInfo = new JiraPayloadData
                {
                    Deployments = new[]
                    {
                        new Extensibility.JiraIntegration.Deployments.JiraDeploymentData
                        {
                            DeploymentSequenceNumber = 11,
                            UpdateSequenceNumber = 3,
                            DisplayName = "Task Name",
                            Associations = new []
                            {
                                new JiraAssociation()
                                {
                                    AssociationType = JiraAssociationConstants.JiraAssociationTypeIssueKeys,
                                    Values = new [] { "JIR-1", "JIR-2"}
                                },
                            },
                            Url =
                                "https://octopussample.com/app#/Spaces-1/projects/foo/releases/1.0.0/deployments/deployments-123",
                            Description = "Task Description",
                            LastUpdated = new DateTimeOffset(new DateTime(2018, 10, 27)),
                            State = "in_progress",
                            Pipeline = new JiraDeploymentPipeline
                            {
                                Id = "Projects-234",
                                DisplayName = "Jira Project",
                                Url = "https://octopussample.com/app#/Spaces-1/projects/foo"
                            },
                            Environment = new JiraDeploymentEnvironment
                            {
                                Id = "env-123",
                                DisplayName = "Development",
                                Type = JiraEnvironmentType.development.ToString()
                            },
                            SchemeVersion = "1.0"
                        }
                    }
                }
            };
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            this.Assent(json);
        }
    }
}