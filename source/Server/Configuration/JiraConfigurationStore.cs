using System;
using Octopus.Data.Storage.Configuration;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Configuration
{
    public class JiraConfigurationStore : ExtensionConfigurationStore<JiraConfiguration>, IJiraConfigurationStore
    {
        public static string CommentParser = "Jira";
        public static string SingletonId = "issuetracker-jira";
        
        public JiraConfigurationStore(IConfigurationStore configurationStore) : base(configurationStore)
        {
        }

        public override string Id => SingletonId;

        public string GetBaseUrl()
        {
            return GetProperty(doc => doc.BaseUrl?.Trim('/'));
        }

        public void SetBaseUrl(string baseUrl)
        {
            SetProperty(doc => doc.BaseUrl = baseUrl?.Trim('/'));
        }

        public string GetPassword()
        {
            return GetProperty(doc => doc.Password);
        }

        public void SetPassword(string password)
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
    }
}
