using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using NSubstitute;
using NUnit.Framework;
using Octopus.CoreUtilities.Extensions;
using Octopus.Data.Model;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.E2E.Tests
{
    internal abstract class ConnectivityCheckActionsBaseFixture
    {
        private const string JiraBaseUrlEnvironmentVariable = "JiraIntegration_E2E_BaseUrl";
        private const string JiraUsernameEnvironmentVariable = "JiraIntegration_E2E_Username";
        private const string JiraAuthTokenEnvironmentVariable = "JiraIntegration_E2E_AuthToken";
        protected IOctopusHttpClientFactory httpClientFactory;
        protected ISystemLog log;
        protected IJiraConfigurationStore store;

        [OneTimeSetUp]
        public void Setup()
        {
            if (!TryGetJiraSettings(out var baseUrl, out var username, out var authToken))
                Assert.Ignore(
                    $"Configure the following environment variables '{JiraBaseUrlEnvironmentVariable}', '{JiraUsernameEnvironmentVariable}', '{JiraAuthTokenEnvironmentVariable}' to run these tests.");

            log = Substitute.For<ISystemLog>();

            store = BuildJiraConfigurationStore(baseUrl, username, authToken);
            httpClientFactory = BuildOctopusHttpClientFactory(baseUrl, username, authToken);
        }

        private static IOctopusHttpClientFactory BuildOctopusHttpClientFactory(string baseUrl, string username,
            string authToken)
        {
            var httpClientFactory = Substitute.For<IOctopusHttpClientFactory>();

            httpClientFactory.CreateClient().Returns(_ =>
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(baseUrl)
                };
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{authToken}")));

                return httpClient;
            });

            return httpClientFactory;
        }

        private static IJiraConfigurationStore BuildJiraConfigurationStore(string baseUrl, string username,
            string authToken, bool isEnabled = true, string releaseNotePrefix = "Release note:")
        {
            var store = Substitute.For<IJiraConfigurationStore>();
            store.GetIsEnabled().Returns(isEnabled);
            store.GetJiraUsername().Returns(username);
            store.GetJiraPassword().Returns(authToken.ToSensitiveString());
            store.GetBaseUrl().Returns(baseUrl);
            store.GetReleaseNotePrefix().Returns(releaseNotePrefix);
            return store;
        }

        private static bool TryGetJiraSettings(out string jiraBaseUrl, out string jiraUsername,
            out string jiraAuthToken)
        {
            jiraBaseUrl = Environment.GetEnvironmentVariable(JiraBaseUrlEnvironmentVariable);
            jiraUsername = Environment.GetEnvironmentVariable(JiraUsernameEnvironmentVariable);
            jiraAuthToken = Environment.GetEnvironmentVariable(JiraAuthTokenEnvironmentVariable);

            return !jiraBaseUrl.IsNullOrEmpty() &&
                   !jiraUsername.IsNullOrEmpty() &&
                   !jiraAuthToken.IsNullOrEmpty();
        }
    }
}