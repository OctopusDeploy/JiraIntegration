using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.JiraIntegration.Deployments;
using Octopus.Server.MessageContracts.Features.BuildInformation;
using Octopus.Server.MessageContracts.Features.IssueTrackers;
using Octopus.Server.MessageContracts.Features.Projects.Releases;
using Octopus.Server.MessageContracts.Features.Projects.Releases.Deployments;

namespace Octopus.Server.Extensibility.JiraIntegration.Tests.JiraDeploymentData
{
    [TestFixture]
    public class JiraIssueTrackerApiDeploymentFixture
    {
        [Test]
        public void WorkItemReferences_AcrossReleases_ShouldBeDistinct()
        {
            var subject = new JiraIssueTrackerApiDeployment();

            var deployment = new DeploymentResource
            {
                Changes = new List<ReleaseChangesResource>
                {
                    CreateChangesForRelease("1.0.0", new Dictionary<string, string[]> { { "TestPackage1", new[] { "JIR-1", "JIR-2" } }, { "TestPackage2", new[] { "JIR-2" } } }),
                    CreateChangesForRelease("1.0.1", new Dictionary<string, string[]> { { "TestPackage1", new[] { "JIR-2", "JIR-3" } }, { "TestPackage2", new[] { "JIR-3" } } })
                }
            };

            var result = subject.DeploymentValues(deployment);
            result.Should().BeEquivalentTo("JIR-1", "JIR-2", "JIR-3");
        }

        ReleaseChangesResource CreateChangesForRelease(string releaseVersion, IDictionary<string, string[]> packageIdToWorkItemIds)
        {
            var buildInformation = packageIdToWorkItemIds.Select(
                    kvp => new ReleasePackageVersionBuildInformation
                    {
                        PackageId = kvp.Key, WorkItems = kvp.Value.Select(wi => new WorkItemLink { Source = JiraConfigurationStore.CommentParser, Id = wi }).ToArray()
                    })
                .ToList();
            return new ReleaseChangesResource
            {
                Version = releaseVersion,
                BuildInformation = buildInformation,
                WorkItems = buildInformation.SelectMany(wi => wi.WorkItems).Distinct().ToList()
            };
        }
    }
}
