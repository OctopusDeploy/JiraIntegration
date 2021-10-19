using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Octopus.Data;
using Octopus.Data.Model;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Server.Extensibility.JiraIntegration.WorkItems;
using Octopus.Server.Extensibility.Results;
using Octopus.Server.MessageContracts.Features.BuildInformation;
using Octopus.Server.MessageContracts.Features.IssueTrackers;

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
                        Comments = new []
                        {
                            new JiraIssueComment
                            {
                                Body = releaseNote
                            }
                        }
                    }
                }
            };
            jiraClient.GetIssues(Arg.Any<string[]>(), CancellationToken.None).Returns(ResultFromExtension<JiraIssue[]>.Success(new [] { jiraIssue }));

            var releaseNoteRegex = new Regex($"^{releaseNotePrefix}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return new WorkItemLinkMapper(store, new CommentParser(), jiraClientLazy, Substitute.For<ISystemLog>()).GetReleaseNote(jiraIssue, releaseNotePrefix, releaseNoteRegex);
        }

        [Test]
        public async Task DuplicatesGetIgnored()
        {
            var store = Substitute.For<IJiraConfigurationStore>();
            var jiraClient = Substitute.For<IJiraRestClient>();
            var jiraClientLazy = new Lazy<IJiraRestClient>(() => jiraClient);
            store.GetBaseUrl(CancellationToken.None).Returns("https://github.com");
            store.GetIsEnabled(CancellationToken.None).Returns(true);
            store.GetJiraUsername(CancellationToken.None).Returns("user");
            store.GetJiraPassword(CancellationToken.None).Returns("password".ToSensitiveString());
            var jiraIssue = new JiraIssue
            {
                Key = "JRE-1234",
                Fields = new JiraIssueFields
                {
                    Comments = new JiraIssueComments()
                }
            };
            jiraClient.GetIssues(Arg.Any<string[]>(), CancellationToken.None).Returns(ResultFromExtension<JiraIssue[]>.Success(new [] {jiraIssue}));

            var mapper = new WorkItemLinkMapper(store, new CommentParser(), jiraClientLazy, Substitute.For<ISystemLog>());

            var workItems = await mapper.Map(new OctopusBuildInformation
            {
                Commits = new Commit[]
                {
                    new Commit { Id = "abcd", Comment = "This is a test commit message. Fixes JRE-1234"},
                    new Commit { Id = "defg", Comment = "This is a test commit message with duplicates. Fixes JRE-1234"}
                }
            }, CancellationToken.None);

            Assert.AreEqual("JRE-1234", ((ISuccessResult<WorkItemLink[]>)workItems).Value.Single().Id, "Single work item should be returned");
        }

        [Test]
        public async Task SourceGetsSet()
        {
            var store = Substitute.For<IJiraConfigurationStore>();
            var jiraClient = Substitute.For<IJiraRestClient>();
            var jiraClientLazy = new Lazy<IJiraRestClient>(() => jiraClient);
            store.GetBaseUrl(CancellationToken.None).Returns("https://github.com");
            store.GetIsEnabled(CancellationToken.None).Returns(true);
            store.GetJiraUsername(CancellationToken.None).Returns("user");
            store.GetJiraPassword(CancellationToken.None).Returns("password".ToSensitiveString());
            var jiraIssue = new JiraIssue
            {
                Key = "JRE-1234",
                Fields = new JiraIssueFields
                {
                    Comments = new JiraIssueComments()
                }
            };
            jiraClient.GetIssues(Arg.Any<string[]>(), CancellationToken.None).Returns(ResultFromExtension<JiraIssue[]>.Success(new [] { jiraIssue }));

            var mapper = new WorkItemLinkMapper(store, new CommentParser(), jiraClientLazy, Substitute.For<ISystemLog>());

            var workItems = await mapper.Map(new OctopusBuildInformation
            {
                Commits = new Commit[]
                {
                    new Commit { Id = "abcd", Comment = "This is a test commit message. Fixes JRE-1234"}
                }
            }, CancellationToken.None);

            Assert.AreEqual("Jira", ((ISuccessResult<WorkItemLink[]>)workItems).Value.Single().Source, "Source should be set");
        }

        [Test]
        public async Task KeyGetsUpperCased()
        {
            var store = Substitute.For<IJiraConfigurationStore>();
            var jiraClient = Substitute.For<IJiraRestClient>();
            var jiraClientLazy = new Lazy<IJiraRestClient>(() => jiraClient);
            store.GetBaseUrl(CancellationToken.None).Returns("https://github.com");
            store.GetIsEnabled(CancellationToken.None).Returns(true);
            store.GetJiraUsername(CancellationToken.None).Returns("user");
            store.GetJiraPassword(CancellationToken.None).Returns("password".ToSensitiveString());
            var jiraIssue = new JiraIssue
            {
                Key = "JRE-1234",
                Fields = new JiraIssueFields
                {
                    Comments = new JiraIssueComments()
                }
            };
            jiraClient.GetIssues(Arg.Any<string[]>(), CancellationToken.None).Returns(ResultFromExtension<JiraIssue[]>.Success(new [] { jiraIssue }));

            var mapper = new WorkItemLinkMapper(store, new CommentParser(), jiraClientLazy, Substitute.For<ISystemLog>());

            var workItems = await mapper.Map(new OctopusBuildInformation
            {
                Commits = new Commit[]
                {
                    new Commit { Id = "abcd", Comment = "This is a test commit message. Fixes jre-1234"}
                }
            }, CancellationToken.None);

            Assert.AreEqual("JRE-1234", ((ISuccessResult<WorkItemLink[]>)workItems).Value.Single().Id);
        }
    }
}