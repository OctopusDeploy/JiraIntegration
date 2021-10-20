using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nito.AsyncEx;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.JiraIntegration.Configuration;
using Octopus.Server.Extensibility.Resources.Configuration;
using Octopus.Server.Extensibility.Results;

namespace Octopus.Server.Extensibility.JiraIntegration.Integration
{
    internal class JiraRestClient : IJiraRestClient, IAsyncDisposable
    {
        private const string BrowseProjectsKey = "BROWSE_PROJECTS";
        private const string BaseApiUri = "rest/api/2";

        private readonly AsyncLazy<HttpClient> lazyClient;
        private readonly ISystemLog systemLog;

        public JiraRestClient(IJiraConfigurationStore store, ISystemLog systemLog,
            IOctopusHttpClientFactory octopusHttpClientFactory)
        {
            this.systemLog = systemLog;

            lazyClient = new AsyncLazy<HttpClient>(() => CreateHttpClient(octopusHttpClientFactory, store),
                AsyncLazyFlags.ExecuteOnCallingThread);
        }

        public JiraRestClient(string baseUrl, string username, string password, ISystemLog systemLog,
            IOctopusHttpClientFactory octopusHttpClientFactory)
        {
            this.systemLog = systemLog;

            lazyClient = new AsyncLazy<HttpClient>(
                () => CreateHttpClient(octopusHttpClientFactory, baseUrl, username, password),
                AsyncLazyFlags.ExecuteOnCallingThread);
        }

        public async ValueTask DisposeAsync()
        {
            if (lazyClient.IsStarted) (await lazyClient).Dispose();
        }

        public async Task<IResultFromExtension<JiraIssue[]>> GetIssues(string[] workItemIds,
            CancellationToken cancellationToken)
        {
            var workItemQuery = $"id in ({string.Join(", ", workItemIds.Select(x => x.ToUpper()))})";
            var content = JsonConvert.SerializeObject(new
                { jql = workItemQuery, fields = new[] { "summary", "comment" }, maxResults = 10000 });
            var httpClient = await lazyClient;

            string errorMessage;
            try
            {
                using var response = await httpClient.PostAsync($"{httpClient.BaseAddress}{BaseApiUri}/search",
                    new StringContent(content, Encoding.UTF8, "application/json"), cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var result = await GetResult<JiraSearchResult>(response, cancellationToken);
                    systemLog.Info(
                        $"Retrieved Jira Work Item data for work item ids {string.Join(", ", result.Issues.Select(wi => wi.Key))}");
                    return ResultFromExtension<JiraIssue[]>.Success(result.Issues);
                }

                errorMessage =
                    $"Failed to retrieve Jira issues '{string.Join(", ", workItemIds)}' from {httpClient.BaseAddress}. Response Code: {response.StatusCode}{(!string.IsNullOrEmpty(response.ReasonPhrase) ? $" (Reason: {response.ReasonPhrase})" : "")}";
            }
            catch (HttpRequestException e)
            {
                errorMessage =
                    $"Failed to retrieve Jira issues '{string.Join(", ", workItemIds)}' from {httpClient.BaseAddress}. (Reason: {e.Message})";
            }
            catch (TaskCanceledException e)
            {
                errorMessage =
                    $"Failed to retrieve Jira issues '{string.Join(", ", workItemIds)}' from {httpClient.BaseAddress}. (Reason: {e.Message})";
            }

            systemLog.Warn(errorMessage);

            return ResultFromExtension<JiraIssue[]>.Failed(errorMessage);
        }

        public async Task<ConnectivityCheckResponse> ConnectivityCheck(CancellationToken cancellationToken)
        {
            var connectivityCheckResponse = new ConnectivityCheckResponse();

            var httpClient = await lazyClient;

            // make sure the user can authenticate
            try
            {
                using var response =
                    await httpClient.GetAsync($"{httpClient.BaseAddress}{BaseApiUri}/myself", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    // make sure the user has browse projects permission
                    using var httpResponseMessage = await httpClient.GetAsync(
                        $"{httpClient.BaseAddress}{BaseApiUri}/mypermissions?permissions={BrowseProjectsKey}",
                        cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                        var permissionsContainer =
                            JsonConvert.DeserializeObject<PermissionSettingsContainer>(jsonContent);

                        if (!permissionsContainer.Permissions.ContainsKey(BrowseProjectsKey))
                        {
                            connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Error,
                                $"Permissions returned from Jira does not contain the {BrowseProjectsKey} permission details.");
                            return connectivityCheckResponse;
                        }

                        var setting = permissionsContainer.Permissions[BrowseProjectsKey];
                        if (!setting.HavePermission)
                        {
                            connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Error,
                                $"User does not have the '{setting.Name}' permission in Jira");
                            return connectivityCheckResponse;
                        }

                        return connectivityCheckResponse;
                    }
                }

                connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Error,
                    $"Failed to connect to {httpClient.BaseAddress}. Response code: {response.StatusCode}{(!string.IsNullOrEmpty(response.ReasonPhrase) ? $" Reason: {response.ReasonPhrase}" : "")}");

                return connectivityCheckResponse;
            }
            catch (HttpRequestException e)
            {
                connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Error,
                    $"Failed to connect to {httpClient.BaseAddress}. Reason: {e.Message}");
                return connectivityCheckResponse;
            }
            catch (TaskCanceledException e)
            {
                connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Error,
                    $"Failed to connect to {httpClient.BaseAddress}. Reason: {e.Message}");
                return connectivityCheckResponse;
            }
        }

        private async Task<TResult> GetResult<TResult>(HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                var result = JsonConvert.DeserializeObject<TResult>(content);
                return result;
            }
            catch (Exception ex)
            {
                response.Headers.TryGetValues("Content-Type", out var contentType);
                var errMsg =
                    $"Error parsing JSON content for type {typeof(TResult)}. Content Type: '{contentType}', content: {content}, error: {ex}";
                systemLog.Error(errMsg);
                throw;
            }
        }

        private static Task<HttpClient> CreateHttpClient(IOctopusHttpClientFactory octopusHttpClientFactory,
            string baseUrl, string username, string? password)
        {
            var client = octopusHttpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));

            return Task.FromResult(client);
        }

        private static async Task<HttpClient> CreateHttpClient(IOctopusHttpClientFactory octopusHttpClientFactory,
            IJiraConfigurationStore store)
        {
            var baseUrl = await store.GetBaseUrl(CancellationToken.None);
            var username = await store.GetJiraUsername(CancellationToken.None);
            var password = (await store.GetJiraPassword(CancellationToken.None))?.ToString();

            return await CreateHttpClient(octopusHttpClientFactory, baseUrl, username, password);
        }
    }

    internal class JiraSearchResult
    {
        [JsonProperty("issues")] public JiraIssue[] Issues { get; set; } = Array.Empty<JiraIssue>();
    }

    internal class JiraIssue
    {
        [JsonProperty("key")] public string Key { get; set; } = string.Empty;

        [JsonProperty("fields")] public JiraIssueFields Fields { get; set; } = new();
    }

    internal class JiraIssueFields
    {
        [JsonProperty("summary")] public string Summary { get; set; } = string.Empty;

        [JsonProperty("comment")] public JiraIssueComments Comments { get; set; } = new();
    }

    internal class JiraIssueComments
    {
        [JsonProperty("comments")]
        public IEnumerable<JiraIssueComment> Comments { get; set; } = new JiraIssueComment[0];

        [JsonProperty("total")] public int Total { get; set; }
    }

    internal class JiraIssueComment
    {
        [JsonProperty("body")] public string? Body { get; set; }
    }

    internal class PermissionSettingsContainer
    {
        [JsonProperty("permissions")] public Dictionary<string, PermissionSettings> Permissions { get; set; } = new();
    }

    internal class PermissionSettings
    {
        public string Name { get; set; } = string.Empty;

        [JsonProperty("havePermission")] public bool HavePermission { get; set; }
    }
}