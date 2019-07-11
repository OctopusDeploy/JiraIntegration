using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octopus.Diagnostics;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Integration
{
    public class JiraRestClient : IJiraRestClient
    {
        private readonly ProductInfoHeaderValue UserAgentHeader = new ProductInfoHeaderValue("octopus-jira-issue-tracker", "1.0");
        private readonly AuthenticationHeaderValue AuthorizationHeader;
        
        private readonly string baseUrl;
        private readonly ILog log;
        private readonly string baseApiUri = "rest/api/2";

        public JiraRestClient(string baseUrl, string username, string password, ILog log)
        {
            this.baseUrl = baseUrl;
            this.log = log;
            AuthorizationHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
        }

        public async Task<JiraIssue> GetIssue(string workItemId)
        {
            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync($"{baseUrl}/{baseApiUri}/issue/{workItemId}?fields=summary,comment");
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<JiraIssue>(await response.Content.ReadAsStringAsync());
                    return result;
                }

                log.Warn($"Failed to retrieve Jira issue '{workItemId}' from {baseUrl}. Status Code: {response.StatusCode}{(!string.IsNullOrEmpty(response.ReasonPhrase) ? $" (Reason: {response.ReasonPhrase})": "")}");
                return null;
            }
        }

        public async Task<JiraIssueComments> GetIssueComments(string workItemId)
        {
            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync($"{baseUrl}/{baseApiUri}/issue/{workItemId}/comment");
                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<JiraIssueComments>(await response.Content.ReadAsStringAsync());

                log.Warn($"Failed to retrieve comments for Jira issue '{workItemId}' from {baseUrl}. Status Code: {response.StatusCode}{(!string.IsNullOrEmpty(response.ReasonPhrase) ? $" (Reason: {response.ReasonPhrase})": "")}");
                return new JiraIssueComments();
            }
        }

        HttpClient CreateHttpClient()
        {
            return new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                DefaultRequestHeaders =
                {
                    Authorization = AuthorizationHeader,
                    UserAgent = {UserAgentHeader},
                },
            };
        }
    }

    public class JiraIssue
    {
        public JiraIssue()
        {
            Fields = new JiraIssueFields();
        }
        
        [JsonProperty("fields")]
        public JiraIssueFields Fields { get; set; }
    }

    public class JiraIssueFields
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

    public class JiraIssueComments
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

    public class JiraIssueComment
    {
        [JsonProperty("body")]
        public string Body { get; set; }
    }
}