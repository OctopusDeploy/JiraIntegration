using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Octopus.Data;
using Octopus.Data.Model;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Model.BuildInformation;
using Octopus.Server.Extensibility.Extensions.WorkItems;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Server.Extensibility.JiraIntegration.WorkItems;
using Octopus.Server.Extensibility.Results;

namespace Octopus.Server.Extensibility.JiraIntegration.Tests
{
    [TestFixture]
    class WorkItemLinkMapperScenarios
    {
        [TestCase("JRE-1234", "Release note:", "Release note: This is the release note", ExpectedResult = "This is the release note")]
        [TestCase("JRE-1234", "Release note:", "release note: This is the release note", ExpectedResult = "This is the release note")]
        [TestCase("JRE-1234", "release note:", "Release note: This is the release note", ExpectedResult = "This is the release note")]
        [TestCase("JRE-1234", "release note:", "release note: This is the release note", ExpectedResult = "This is the release note")]
        [TestCase("JRE-1234", "Release note:", "Release note: This is the release note", ExpectedResult = "This is the release note")]
        [TestCase("JRE-1234", "release note:", "Release note: This is the release note", ExpectedResult = "This is the release note")]
        [TestCase("JRE-1234", "Release note:", "release note: This is the release note", ExpectedResult = "This is the release note")]
        [TestCase("JRE-1234", "release note:", "release note: This is the release note", ExpectedResult = "This is the release note")]
        [TestCase("JRE-1234", "Release note:", "This is not a release note", ExpectedResult = "Test title")]
        [TestCase("JRE-1234", "", "Release note: This is a release note", ExpectedResult = "Test title")]
        [TestCase("JRE-1234", "Release note:", "Release notes: This is the release note", ExpectedResult = "Test title")]
        [TestCase("JRE-1234", "Release note:", "release notes: This is the release note", ExpectedResult = "Test title")]
        public string GetWorkItemDescription(string linkData, string releaseNotePrefix, string releaseNote)
        {
            var store = Substitute.For<IJiraConfigurationStore>();
            var jiraClient = Substitute.For<IJiraRestClient>();
            var jiraClientLazy = new Lazy<IJiraRestClient>(() => jiraClient);
            var jiraIssue = new JiraIssue
            {
                Key = linkData,
                Fields = new JiraIssueFields
                {
                    Summary = "Test title",
                    Comments = new JiraIssueComments
                    {
                        Total = 1,
                        Comments = new[]
                        {
                            new JiraIssueComment
                            {
                                Body = releaseNote
                            }
                        }
                    }
                }
            };
            jiraClient.GetIssues(Arg.Any<string[]>()).Returns(ResultFromExtension<JiraIssue[]>.Success(new[] { jiraIssue }));

            return new WorkItemLinkMapper(store, new CommentParser(), jiraClientLazy, Substitute.For<ISystemLog>()).GetReleaseNote(jiraIssue, releaseNotePrefix);
        }

        [Test]
        public void DuplicatesGetIgnored()
        {
            var store = Substitute.For<IJiraConfigurationStore>();
            var jiraClient = Substitute.For<IJiraRestClient>();
            var jiraClientLazy = new Lazy<IJiraRestClient>(() => jiraClient);
            store.GetBaseUrl().Returns("https://github.com");
            store.GetIsEnabled().Returns(true);
            store.GetJiraUsername().Returns("user");
            store.GetJiraPassword().Returns("password".ToSensitiveString());
            var jiraIssue = new JiraIssue
            {
                Key = "JRE-1234",
                Fields = new JiraIssueFields
                {
                    Comments = new JiraIssueComments()
                }
            };
            jiraClient.GetIssues(Arg.Any<string[]>()).Returns(ResultFromExtension<JiraIssue[]>.Success(new[] { jiraIssue }));

            var mapper = new WorkItemLinkMapper(store, new CommentParser(), jiraClientLazy, Substitute.For<ISystemLog>());

            var workItems = mapper.Map(
                new OctopusBuildInformation
                {
                    Commits = new[]
                    {
                        new()
                            { Id = "abcd", Comment = "This is a test commit message. Fixes JRE-1234" },
                        new Commit { Id = "defg", Comment = "This is a test commit message with duplicates. Fixes JRE-1234" }
                    }
                });

            Assert.AreEqual("JRE-1234", ((ISuccessResult<WorkItemLink[]>)workItems).Value.Single().Id, "Single work item should be returned");
        }

        [Test]
        public void SourceGetsSet()
        {
            var store = Substitute.For<IJiraConfigurationStore>();
            var jiraClient = Substitute.For<IJiraRestClient>();
            var jiraClientLazy = new Lazy<IJiraRestClient>(() => jiraClient);
            store.GetBaseUrl().Returns("https://github.com");
            store.GetIsEnabled().Returns(true);
            store.GetJiraUsername().Returns("user");
            store.GetJiraPassword().Returns("password".ToSensitiveString());
            var jiraIssue = new JiraIssue
            {
                Key = "JRE-1234",
                Fields = new JiraIssueFields
                {
                    Comments = new JiraIssueComments()
                }
            };
            jiraClient.GetIssues(Arg.Any<string[]>()).Returns(ResultFromExtension<JiraIssue[]>.Success(new[] { jiraIssue }));

            var mapper = new WorkItemLinkMapper(store, new CommentParser(), jiraClientLazy, Substitute.For<ISystemLog>());

            var workItems = mapper.Map(
                new OctopusBuildInformation
                {
                    Commits = new[]
                    {
                        new Commit { Id = "abcd", Comment = "This is a test commit message. Fixes JRE-1234" }
                    }
                });

            Assert.AreEqual("Jira", ((ISuccessResult<WorkItemLink[]>)workItems).Value.Single().Source, "Source should be set");
        }

        [Test]
        public void KeyGetsUpperCased()
        {
            var store = Substitute.For<IJiraConfigurationStore>();
            var jiraClient = Substitute.For<IJiraRestClient>();
            var jiraClientLazy = new Lazy<IJiraRestClient>(() => jiraClient);
            store.GetBaseUrl().Returns("https://github.com");
            store.GetIsEnabled().Returns(true);
            store.GetJiraUsername().Returns("user");
            store.GetJiraPassword().Returns("password".ToSensitiveString());
            var jiraIssue = new JiraIssue
            {
                Key = "JRE-1234",
                Fields = new JiraIssueFields
                {
                    Comments = new JiraIssueComments()
                }
            };
            jiraClient.GetIssues(Arg.Any<string[]>()).Returns(ResultFromExtension<JiraIssue[]>.Success(new[] { jiraIssue }));

            var mapper = new WorkItemLinkMapper(store, new CommentParser(), jiraClientLazy, Substitute.For<ISystemLog>());

            var workItems = mapper.Map(
                new OctopusBuildInformation
                {
                    Commits = new[]
                    {
                        new Commit { Id = "abcd", Comment = "This is a test commit message. Fixes jre-1234" }
                    }
                });

            Assert.AreEqual("JRE-1234", ((ISuccessResult<WorkItemLink[]>)workItems).Value.Single().Id);
        }
    }
}
