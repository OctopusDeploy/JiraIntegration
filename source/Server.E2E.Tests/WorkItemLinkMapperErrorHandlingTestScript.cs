using System;
using NSubstitute;
using NUnit.Framework;
using Octopus.Diagnostics;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Server.Extensibility.JiraIntegration.WorkItems;
using Octopus.Server.Extensibility.Results;
using Octopus.Server.MessageContracts.Features.BuildInformation;
using Octopus.Server.MessageContracts.Features.IssueTrackers;
using Shouldly;

namespace Octopus.Server.Extensibility.JiraIntegration.E2E.Tests
{
    [TestFixture]
    public class WorkItemLinkMapperErrorHandlingTestScript : WorkItemMapperBaseFixture
    {
        [Test]
        public void HttpAuthExceptionShouldLogMeaningfulMessage()
        {
            if (!TryGetJiraSettings(out var baseUrl, out var username, out var _))
                Assert.Ignore(
                    $"Configure the following environment variables '{JiraBaseUrlEnvironmentVariable}', '{JiraUsernameEnvironmentVariable}', '{JiraAuthTokenEnvironmentVariable}' to run these tests.");

            var log = Substitute.For<ISystemLog>();

            var store = BuildJiraConfigurationStore(baseUrl, username, "invalidtoken");
            var jira = BuildJiraRestClient(baseUrl, username, "invalidtoken", log);

            var buildInformation = CreateBuildInformation(new[]
            {
                new Commit
                {
                    Id = "234",
                    Comment = "OATP-9999"
                }
            });

            var workItemLinkMapper = new WorkItemLinkMapper(
                store,
                new CommentParser(),
                new Lazy<IJiraRestClient>(jira),
                log);

            IResultFromExtension<WorkItemLink[]> result = null;
            Should.NotThrow(
                () => { result = workItemLinkMapper.Map(buildInformation); });

            result.ShouldBeOfType<FailureResultFromExtension<WorkItemLink[]>>("A failure should be received");
            var failure = (FailureResultFromExtension<WorkItemLink[]>)result;
            failure.ErrorString.ShouldStartWith(
                "Authentication failure, check the Jira access token is valid and has permissions to read work items");
        }
    }
}