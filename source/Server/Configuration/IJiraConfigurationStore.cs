using Octopus.Data.Model;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.JiraIntegration.Configuration
{
    interface IJiraConfigurationStore : IExtensionConfigurationStore<JiraConfiguration>
    {
        JiraInstanceType GetJiraInstanceType();
        void SetJiraInstanceType(JiraInstanceType jiraInstanceType);
        string GetBaseUrl();
        void SetBaseUrl(string baseUrl);
        
        SensitiveString GetConnectAppPassword();
        void SetConnectAppPassword(SensitiveString password);
        
        string GetConnectAppUrl();
        void SetConnectAppUrl(string url);
        
        string GetJiraUsername();
        void SetJiraUsername(string username);
        
        SensitiveString GetJiraPassword();
        void SetJiraPassword(SensitiveString password);
        
        string GetReleaseNotePrefix();
        void SetReleaseNotePrefix(string releaseNotePrefix);
    }
}
