using System;
using NSubstitute;
using NUnit.Framework;
using Octopus.Server.Extensibility.HostServices.Model.PackageMetadata;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;
using Octopus.Server.Extensibility.IssueTracker.Jira.WorkItems;
using Octopus.Server.Extensibility.IssueTracker.Jira.Integration;
using Commit = Octopus.Server.Extensibility.HostServices.Model.IssueTrackers.Commit;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Tests
{
    [TestFixture]
    public class WorkItemLinkMapperScenarios
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
                        Total = 1
                    }
                }
            };
            jiraClient.GetIssue(Arg.Is(linkData)).Returns(jiraIssue);
            jiraClient.GetIssueComments(Arg.Is(linkData)).Returns(new JiraIssueComments
            {
                Comments = new [] {new JiraIssueComment { Body = releaseNote }}
            });

            return new WorkItemLinkMapper(store, new CommentParser(), jiraClientLazy).GetReleaseNote(jiraIssue, releaseNotePrefix);
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
            store.GetJiraPassword().Returns("password");
            jiraClient.GetIssue(Arg.Is("JRE-1234")).Returns(new JiraIssue());
            jiraClient.GetIssueComments(Arg.Is("JRE-1234")).Returns(new JiraIssueComments
            {
                Comments = new [] {new JiraIssueComment { Body = string.Empty }}
            });
            
            var mapper = new WorkItemLinkMapper(store, new CommentParser(), jiraClientLazy);

            var workItems = mapper.Map(new OctopusPackageMetadata
            {
                CommentParser = "Jira",
                Commits = new Commit[]
                {
                    new Commit { Id = "abcd", Comment = "This is a test commit message. Fixes JRE-1234"},
                    new Commit { Id = "defg", Comment = "This is a test commit message with duplicates. Fixes JRE-1234"}
                }
            });

            Assert.IsTrue(workItems.Succeeded);
            Assert.AreEqual(1, workItems.Value.Length);
        }
    }
}