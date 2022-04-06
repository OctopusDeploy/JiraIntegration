using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using NSubstitute;
using Octopus.CoreUtilities.Extensions;
using Octopus.Data.Model;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Server.MessageContracts.Features.BuildInformation;

namespace Octopus.Server.Extensibility.JiraIntegration.E2E.Tests
{
    public class WorkItemMapperBaseFixture
    {
        protected const string JiraBaseUrlEnvironmentVariable = "JiraIntegration_E2E_BaseUrl";
        protected const string JiraUsernameEnvironmentVariable = "JiraIntegration_E2E_Username";
        protected const string JiraAuthTokenEnvironmentVariable = "JiraIntegration_E2E_AuthToken";

        internal static JiraRestClient BuildJiraRestClient(string baseUrl, string username, string authToken, ISystemLog log)
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

        internal static IJiraConfigurationStore BuildJiraConfigurationStore(string baseUrl, string username, string authToken, bool isEnabled = true, string releaseNotePrefix = "Release note:")
        {
            var store = Substitute.For<IJiraConfigurationStore>();
            store.GetIsEnabled().Returns(isEnabled);
            store.GetJiraUsername().Returns(username);
            store.GetJiraPassword().Returns(authToken.ToSensitiveString());
            store.GetBaseUrl().Returns(baseUrl);
            store.GetReleaseNotePrefix().Returns(releaseNotePrefix);
            return store;
        }

        protected static bool TryGetJiraSettings(out string jiraBaseUrl, out string jiraUsername, out string jiraAuthToken)
        {
            jiraBaseUrl = Environment.GetEnvironmentVariable(JiraBaseUrlEnvironmentVariable);
            jiraUsername = Environment.GetEnvironmentVariable(JiraUsernameEnvironmentVariable);
            jiraAuthToken = Environment.GetEnvironmentVariable(JiraAuthTokenEnvironmentVariable);

            return !jiraBaseUrl.IsNullOrEmpty() &&
                !jiraUsername.IsNullOrEmpty() &&
                !jiraAuthToken.IsNullOrEmpty();
        }

        internal static OctopusBuildInformation CreateBuildInformation(Commit[] commits)
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
