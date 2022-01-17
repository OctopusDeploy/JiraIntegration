using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using NSubstitute;
using NUnit.Framework;
using Octopus.CoreUtilities.Extensions;
using Octopus.Data.Model;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.Extensions.WorkItems;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Server.Extensibility.JiraIntegration.WorkItems;
using Octopus.Server.Extensibility.Results;
using Octopus.Server.MessageContracts.Features.BuildInformation;
using Octopus.Server.MessageContracts.Features.IssueTrackers;

namespace Octopus.Server.Extensibility.JiraIntegration.E2E.Tests
{
    [TestFixture]
    public class WorkItemLinkMapperTestScript
    {
        const string JiraBaseUrlEnvironmentVariable = "JiraIntegration_E2E_BaseUrl";
        const string JiraUsernameEnvironmentVariable = "JiraIntegration_E2E_Username";
        const string JiraAuthTokenEnvironmentVariable = "JiraIntegration_E2E_AuthToken";

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
            var buildInformation = CreateBuildInformation(new[]
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
            var buildInformation = CreateBuildInformation(new[]
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
            var buildInformation = CreateBuildInformation(new[]
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

        static JiraRestClient BuildJiraRestClient(string baseUrl, string username, string authToken, ISystemLog log)
        {
            var httpClientFactory = BuildOctopusHttpClientFactory(baseUrl, username, authToken);

            return new JiraRestClient(
                                      baseUrl,
                                      username,
                                      authToken,
                                      log,
                                      httpClientFactory);
        }

        static IOctopusHttpClientFactory BuildOctopusHttpClientFactory(string baseUrl, string username, string authToken)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                                                                                           Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{authToken}")));

            var httpClientFactory = Substitute.For<IOctopusHttpClientFactory>();
            httpClientFactory.CreateClient().Returns(httpClient);

            return httpClientFactory;
        }

        static IJiraConfigurationStore BuildJiraConfigurationStore(string baseUrl,
                                                                   string username,
                                                                   string authToken,
                                                                   bool isEnabled = true,
                                                                   string releaseNotePrefix = "Release note:")
        {
            var store = Substitute.For<IJiraConfigurationStore>();
            store.GetIsEnabled().Returns(isEnabled);
            store.GetJiraUsername().Returns(username);
            store.GetJiraPassword().Returns(authToken.ToSensitiveString());
            store.GetBaseUrl().Returns(baseUrl);
            store.GetReleaseNotePrefix().Returns(releaseNotePrefix);
            return store;
        }

        static bool TryGetJiraSettings(out string jiraBaseUrl, out string jiraUsername, out string jiraAuthToken)
        {
            jiraBaseUrl = Environment.GetEnvironmentVariable(JiraBaseUrlEnvironmentVariable);
            jiraUsername = Environment.GetEnvironmentVariable(JiraUsernameEnvironmentVariable);
            jiraAuthToken = Environment.GetEnvironmentVariable(JiraAuthTokenEnvironmentVariable);

            return !jiraBaseUrl.IsNullOrEmpty() && !jiraUsername.IsNullOrEmpty() && !jiraAuthToken.IsNullOrEmpty();
        }

        static void AssertIssueWasReturnedAndHasCorrectDetails(string issueId, string expectedDescription, IEnumerable<WorkItemLink> issues)
        {
            var issue =
                issues.FirstOrDefault(v => v.Id.Equals(issueId, StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(issue);
            Assert.AreEqual(expectedDescription, issue.Description);
        }

        static OctopusBuildInformation CreateBuildInformation(Commit[] commits)
        {
            return new OctopusBuildInformation
            {
                VcsType = "Git",
                VcsRoot = "https://github.com/testOrg/testRepo",
                Branch = "main",
                BuildEnvironment = "buildserverX",
                Commits = commits
            };
        }
    }
}