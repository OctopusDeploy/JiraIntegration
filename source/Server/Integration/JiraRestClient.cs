using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.IssueTracker.Jira.Web.Response;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Integration
{
    class JiraRestClient : IJiraRestClient
    {
        const string BrowseProjectsKey = "BROWSE_PROJECTS";

        private readonly AuthenticationHeaderValue AuthorizationHeader;
        private readonly HttpClient httpClient;

        private readonly string baseUrl;
        private readonly ILog log;
        private readonly string baseApiUri = "rest/api/2";

        public JiraRestClient(string baseUrl, string username, string password, ILog log,
            IOctopusHttpClientFactory octopusHttpClientFactory)
        {
            this.baseUrl = baseUrl;
            this.log = log;
            AuthorizationHeader = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
            httpClient = CreateHttpClient(octopusHttpClientFactory);
        }

        public async Task<ConnectivityCheckResponse> ConnectivityCheck()
        {
            // make sure the user can authenticate
            var response = await httpClient.GetAsync($"{baseUrl}/{baseApiUri}/myself");
            if (response.IsSuccessStatusCode)
            {
                // make sure the user has browse projects permission
                response = await httpClient.GetAsync(
                    $"{baseUrl}/{baseApiUri}/mypermissions?permissions={BrowseProjectsKey}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var permissionsContainer = JsonConvert.DeserializeObject<PermissionSettingsContainer>(jsonContent);

                    if (!permissionsContainer.permissions.ContainsKey(BrowseProjectsKey))
                        return ConnectivityCheckResponse.Failure(
                            $"Permissions returned from Jira does not contain the {BrowseProjectsKey} permission details.");

                    var setting = permissionsContainer.permissions[BrowseProjectsKey];
                    if (!setting.havePermission)
                        return ConnectivityCheckResponse.Failure(
                            $"User does not have the '{setting.Name}' permission in Jira");

                    return ConnectivityCheckResponse.Success;
                }
            }

            return ConnectivityCheckResponse.Failure(
                $"Failed to connect to {baseUrl}. Response code: {response.StatusCode}{(!string.IsNullOrEmpty(response.ReasonPhrase) ? $"Reason: {response.ReasonPhrase}" : "")}");
        }

        public async Task<JiraIssue> GetIssue(string workItemId)
        {
            var response =
                await httpClient.GetAsync($"{baseUrl}/{baseApiUri}/issue/{workItemId}?fields=summary,comment");
            if (response.IsSuccessStatusCode)
            {
                var result = await GetResult<JiraIssue>(response);
                return result;
            }

            var msg =
                $"Failed to retrieve Jira issue '{workItemId}' from {baseUrl}. Response Code: {response.StatusCode}{(!string.IsNullOrEmpty(response.ReasonPhrase) ? $" (Reason: {response.ReasonPhrase})" : "")}";
            if (response.StatusCode == HttpStatusCode.NotFound)
                log.Trace(msg);
            else
                log.Warn(msg);
            return null;
        }

        public async Task<JiraIssueComments> GetIssueComments(string workItemId)
        {
            var response = await httpClient.GetAsync($"{baseUrl}/{baseApiUri}/issue/{workItemId}/comment");
            if (response.IsSuccessStatusCode)
                return await GetResult<JiraIssueComments>(response);

            var msg =
                $"Failed to retrieve comments for Jira issue '{workItemId}' from {baseUrl}. Response Code: {response.StatusCode}{(!string.IsNullOrEmpty(response.ReasonPhrase) ? $" (Reason: {response.ReasonPhrase})" : "")}";
            if (response.StatusCode == HttpStatusCode.NotFound)
                log.Trace(msg);
            else
                log.Warn(msg);
            return new JiraIssueComments();
        }

        async Task<TResult> GetResult<TResult>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            try

            {
                var result = JsonConvert.DeserializeObject<TResult>(content);
                return result;
            }
            catch
            {
                response.Headers.TryGetValues("Content-Type", out var contentType);
                var errMsg =
                    $"Error parsing JSON content for type {typeof(TResult)}. Content Type: '{contentType}', content: {content}";
                log.Error(errMsg);
                throw;
            }
        }

        HttpClient CreateHttpClient(IOctopusHttpClientFactory octopusHttpClientFactory)
        {
            var client = octopusHttpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Authorization = AuthorizationHeader;
            return client;
        }
    }

    class JiraIssue
    {
        public JiraIssue()
        {
            Fields = new JiraIssueFields();
        }
        
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("fields")]
        public JiraIssueFields Fields { get; set; }
    }

    class JiraIssueFields
    {
        public JiraIssueFields()
        {
            Comments = new JiraIssueComments();
        }
        
        [JsonProperty("summary")]
        public string Summary { get; set; }
        [JsonProperty("comment")]
        public JiraIssueComments Comments { get; set; }
    }

    class JiraIssueComments
    {
        public JiraIssueComments()
        {
            Comments = new List<JiraIssueComment>();
        }
        
        [JsonProperty("comments")]
        public IEnumerable<JiraIssueComment> Comments { get; set; }
        [JsonProperty("total")]
        public int Total { get; set; }
    }

    class JiraIssueComment
    {
        [JsonProperty("body")]
        public string Body { get; set; }
    }
    
    class PermissionSettingsContainer
    {
        public Dictionary<string, PermissionSettings> permissions { get; set; }
    }

    class PermissionSettings
    {
        public string Name { get; set; }
        public bool havePermission { get; set; }
    }
}