using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.HostServices.Licensing;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Server.Extensibility.JiraIntegration.Web;
using Octopus.Server.Extensibility.Resources.Configuration;
using Shouldly;

namespace Octopus.Server.Extensibility.JiraIntegration.E2E.Tests
{
    [TestFixture]
    class JiraConnectAppConnectivityCheckActionFixture : ConnectivityCheckActionsBaseFixture
    {
        [Test]
        public async Task WhenDnsCannotBeResolved()
        {
            var installationIdProvider = Substitute.For<IInstallationIdProvider>();
            installationIdProvider.GetInstallationId().Returns(Guid.NewGuid());

            var baseUrl = "http://notexistingdomain.dddd.ttt";
            store.GetConnectAppUrl().Returns(baseUrl);

            var action = new JiraConnectAppConnectivityCheckAction(log, store, installationIdProvider, new JiraConnectAppClient(installationIdProvider, store, httpClientFactory), httpClientFactory);
            var octoRequest = Substitute.For<IOctoRequest>();
            octoRequest.GetBody(Arg.Any<RequestBodyRegistration<JiraConnectAppConnectionCheckData>>())
                .Returns(new JiraConnectAppConnectionCheckData
                    { BaseUrl = baseUrl, Password = "Does not matter" });

            var response = await action.ExecuteAsync(octoRequest);

            var connectivityCheckResponse = (ConnectivityCheckResponse)((OctoDataResponse)response.Response).Model;

            connectivityCheckResponse.Messages.ShouldNotBeEmpty();
            connectivityCheckResponse.Messages.First().Message.ShouldBe("Failed to get authentication token from Jira Connect App.");
        }
    }
}