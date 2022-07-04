using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Model.BuildInformation;
using Octopus.Server.Extensibility.Extensions.WorkItems;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Server.Extensibility.JiraIntegration.WorkItems;
using Octopus.Server.Extensibility.Results;

namespace Octopus.Server.Extensibility.JiraIntegration.E2E.Tests
{
    [TestFixture]
    public class WorkItemLinkMapperTestScript : WorkItemMapperBaseFixture
    {
        IWorkItemLinkMapper workItemLinkMapper;

        [OneTimeSetUp]
        public void Setup()
        {
            if (!TryGetJiraSettings(out var baseUrl, out var username, out var authToken))
                Assert.Ignore($"Configure the following environment variables '{JiraBaseUrlEnvironmentVariable}', '{JiraUsernameEnvironmentVariable}', '{JiraAuthTokenEnvironmentVariable}' to run these tests.");

            var log = Substitute.For<ISystemLog>();

            var store = BuildJiraConfigurationStore(baseUrl, username, authToken);
            var jira = BuildJiraRestClient(baseUrl, username, authToken, log);

            workItemLinkMapper = new WorkItemLinkMapper(
                store,
                new CommentParser(),
                new Lazy<IJiraRestClient>(jira),
                log);
        }

        [Test]
        public void WeCanDeserializeJiraIssuesWithComments()
        {
            var buildInformation = CreateBuildInformation(
                new[]
                {
                    new Commit
                    {
                        Id = "123",
                        Comment = "OATP-1"
                    },
                    new Commit
                    {
                        Id = "234",
                        Comment = "OATP-9"
                    }
                });

            var result = (ResultFromExtension<WorkItemLink[]>)workItemLinkMapper.Map(buildInformation);

            Assert.NotNull(result.Value);
            Assert.AreEqual(2, result.Value.Length);

            AssertIssueWasReturnedAndHasCorrectDetails("OATP-1", "Test issue 1", result.Value);
            AssertIssueWasReturnedAndHasCorrectDetails("OATP-9", "This is a release note for Test issue 9", result.Value);
        }

        [Test]
        public void WeCanDeserializeJiraIssuesWhenOnlySomeIssuesAreFound()
        {
            var buildInformation = CreateBuildInformation(
                new[]
                {
                    new Commit
                    {
                        Id = "123",
                        Comment = "OATP-1"
                    },
                    new Commit
                    {
                        Id = "234",
                        Comment = "OATP-9999" // non-existent
                    }
                });

            var result = (ResultFromExtension<WorkItemLink[]>)workItemLinkMapper.Map(buildInformation);

            Assert.NotNull(result.Value);
            Assert.AreEqual(1, result.Value.Length);

            AssertIssueWasReturnedAndHasCorrectDetails("OATP-1", "Test issue 1", result.Value);
        }

        [Test]
        public void WeCanDeserializeJiraIssuesAsEmptyCollectionWhenNoIssuesAreFound()
        {
            var buildInformation = CreateBuildInformation(
                new[]
                {
                    new Commit
                    {
                        Id = "234",
                        Comment = "OATP-9999" // non-existent
                    }
                });

            var result = (ResultFromExtension<WorkItemLink[]>)workItemLinkMapper.Map(buildInformation);

            Assert.NotNull(result.Value);
            Assert.AreEqual(0, result.Value.Length);
        }

        static void AssertIssueWasReturnedAndHasCorrectDetails(string issueId, string expectedDescription, IEnumerable<WorkItemLink> issues)
        {
            var issue =
                issues.FirstOrDefault(v => v.Id.Equals(issueId, StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(issue);
            Assert.AreEqual(expectedDescription, issue.Description);
        }
    }
}
