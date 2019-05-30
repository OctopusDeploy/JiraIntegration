using System;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Configuration
{
    public interface IJiraConfigurationStore : IExtensionConfigurationStore<JiraConfiguration>
    {
        JiraInstanceType GetJiraInstanceType();
        void SetJiraInstanceType(JiraInstanceType jiraInstanceType);
        string GetBaseUrl();
        void SetBaseUrl(string baseUrl);
        string GetConnectAppPassword();
        void SetConnectAppPassword(string password);
        string GetConnectAppUrl();
        void SetConnectAppUrl(string url);
        string GetJiraUsername();
        void SetJiraUsername(string username);
        string GetJiraPassword();
        void SetJiraPassword(string password);
        string GetReleaseNotePrefix();
        void SetReleaseNotePrefix(string releaseNotePrefix);
    }
}
