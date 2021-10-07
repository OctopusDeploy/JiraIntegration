using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.Resources.Configuration;
using Octopus.Server.Extensibility.Results;

namespace Octopus.Server.Extensibility.JiraIntegration.Integration
{
    internal class JiraRestClient : IJiraRestClient, IDisposable
    {
        private const string BrowseProjectsKey = "BROWSE_PROJECTS";

        private readonly AuthenticationHeaderValue authorizationHeader;
        private readonly string baseApiUri = "rest/api/2";

        private readonly string baseUrl;
        private readonly HttpClient httpClient;
        private readonly ISystemLog systemLog;

        public JiraRestClient(string baseUrl, string username, string? password, ISystemLog systemLog,
            IOctopusHttpClientFactory octopusHttpClientFactory)
        {
            this.baseUrl = baseUrl;
            this.systemLog = systemLog;
            authorizationHeader = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
            httpClient = CreateHttpClient(octopusHttpClientFactory);
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }

        public async Task<ConnectivityCheckResponse> ConnectivityCheck()
        {
            var connectivityCheckResponse = new ConnectivityCheckResponse();
            // make sure the user can authenticate
            try
            {
                using var response = await httpClient.GetAsync($"{baseUrl}/{baseApiUri}/myself");
                if (response.IsSuccessStatusCode)
                {
                    // make sure the user has browse projects permission
                    using var httpResponseMessage =
                        await httpClient.GetAsync(
                            $"{baseUrl}/{baseApiUri}/mypermissions?permissions={BrowseProjectsKey}");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = await httpResponseMessage.Content.ReadAsStringAsync();
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
                    $"Failed to connect to {baseUrl}. Response code: {response.StatusCode}{(!string.IsNullOrEmpty(response.ReasonPhrase) ? $" Reason: {response.ReasonPhrase}" : "")}");

                return connectivityCheckResponse;
            }
            catch (HttpRequestException e)
            {
                connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Error,
                    $"Failed to connect to {baseUrl}. Reason: {e.Message}");
                return connectivityCheckResponse;
            }
            catch (TaskCanceledException e)
            {
                connectivityCheckResponse.AddMessage(ConnectivityCheckMessageCategory.Error,
                    $"Failed to connect to {baseUrl}. Reason: {e.Message}");
                return connectivityCheckResponse;
            }
        }

        public async Task<IResultFromExtension<JiraIssue[]>> GetIssues(string[] workItemIds)
        {
            var workItemQuery = $"id in ({string.Join(", ", workItemIds.Select(x => x.ToUpper()))})";
            var content = JsonConvert.SerializeObject(new
                { jql = workItemQuery, fields = new[] { "summary", "comment" }, maxResults = 10000 });

            string errorMessage;
            try
            {
                using var response = await httpClient.PostAsync($"{baseUrl}/{baseApiUri}/search",
                    new StringContent(content, Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var result = await GetResult<JiraSearchResult>(response);
                    systemLog.Info(
                        $"Retrieved Jira Work Item data for work item ids {string.Join(", ", result.Issues.Select(wi => wi.Key))}");
                    return ResultFromExtension<JiraIssue[]>.Success(result.Issues);
                }

                errorMessage =
                    $"Failed to retrieve Jira issues '{string.Join(", ", workItemIds)}' from {baseUrl}. Response Code: {response.StatusCode}{(!string.IsNullOrEmpty(response.ReasonPhrase) ? $" (Reason: {response.ReasonPhrase})" : "")}";
            }
            catch (HttpRequestException e)
            {
                errorMessage =
                    $"Failed to retrieve Jira issues '{string.Join(", ", workItemIds)}' from {baseUrl}. (Reason: {e.Message})";
            }
            catch (TaskCanceledException e)
            {
                errorMessage =
                    $"Failed to retrieve Jira issues '{string.Join(", ", workItemIds)}' from {baseUrl}. (Reason: {e.Message})";
            }

            systemLog.Warn(errorMessage);

            return ResultFromExtension<JiraIssue[]>.Failed(errorMessage);
        }

        private async Task<TResult> GetResult<TResult>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
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

        private HttpClient CreateHttpClient(IOctopusHttpClientFactory octopusHttpClientFactory)
        {
            var client = octopusHttpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Authorization = authorizationHeader;
            return client;
        }
    }

    internal class JiraSearchResult
    {
        [JsonProperty("issues")] public JiraIssue[] Issues { get; set; } = new JiraIssue[0];
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