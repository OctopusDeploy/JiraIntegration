using System;
using Octopus.Data.Storage.Configuration;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Configuration
{
    class JiraConfigurationStore : ExtensionConfigurationStore<JiraConfiguration>, IJiraConfigurationStore
    {
        public static string CommentParser = "Jira";
        public static string SingletonId = "issuetracker-jira";
        
        public JiraConfigurationStore(IConfigurationStore configurationStore) : base(configurationStore)
        {
        }

        public override string Id => SingletonId;

        public JiraInstanceType GetJiraInstanceType()
        {
            return GetProperty(doc => doc.JiraInstanceType);
        }

        public void SetJiraInstanceType(JiraInstanceType jiraInstanceType)
        {
            SetProperty(doc => doc.JiraInstanceType = jiraInstanceType);
        }

        public string GetBaseUrl()
        {
            return GetProperty(doc => doc.BaseUrl?.Trim('/'));
        }

        public void SetBaseUrl(string baseUrl)
        {
            SetProperty(doc => doc.BaseUrl = baseUrl?.Trim('/'));
        }

        public string GetConnectAppPassword()
        {
            return GetProperty(doc => doc.Password);
        }

        public void SetConnectAppPassword(string password)
        {
            SetProperty(doc => doc.Password = password);
        }

        public string GetConnectAppUrl()
        {
            return GetProperty(doc => doc.ConnectAppUrl?.Trim('/'));
        }

        public void SetConnectAppUrl(string url)
        {
            SetProperty(doc => doc.ConnectAppUrl = url?.Trim('/'));
        }

        public string GetJiraUsername()
        {
            return GetProperty(doc => doc.ReleaseNoteOptions.Username);
        }

        public void SetJiraUsername(string username)
        {
            SetProperty(doc => doc.ReleaseNoteOptions.Username = username);
        }

        public string GetJiraPassword()
        {
            return GetProperty(doc => doc.ReleaseNoteOptions.Password);
        }

        public void SetJiraPassword(string password)
        {
            SetProperty(doc => doc.ReleaseNoteOptions.Password = password);
        }

        public string GetReleaseNotePrefix()
        {
            return GetProperty(doc => doc.ReleaseNoteOptions.ReleaseNotePrefix);
        }

        public void SetReleaseNotePrefix(string releaseNotePrefix)
        {
            SetProperty(doc => doc.ReleaseNoteOptions.ReleaseNotePrefix = releaseNotePrefix);
        }
    }
}
