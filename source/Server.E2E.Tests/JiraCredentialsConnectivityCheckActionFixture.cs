using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.JiraIntegration.Web;
using Octopus.Server.Extensibility.Resources.Configuration;
using Shouldly;

namespace Octopus.Server.Extensibility.JiraIntegration.E2E.Tests
{
    [TestFixture]
    class JiraCredentialsConnectivityCheckActionFixture : ConnectivityCheckActionsBaseFixture
    {
        [Test]
        public async Task WhenDnsCannotBeResolved()
        {
            var action = new JiraCredentialsConnectivityCheckAction(store, httpClientFactory, log);
            var octoRequest = Substitute.For<IOctoRequest>();
            var baseUrl = "http://notexistingdomain.dddd.ttt";
            octoRequest.GetBody(Arg.Any<RequestBodyRegistration<JiraCredentialsConnectionCheckData>>())
                       .Returns(new JiraCredentialsConnectionCheckData { BaseUrl = baseUrl, Username = "Does not matter", Password = "Does not matter" });

            var response = await action.ExecuteAsync(octoRequest);

            var connectivityCheckResponse = (ConnectivityCheckResponse)((OctoDataResponse)response.Response).Model;

            connectivityCheckResponse.Messages.ShouldNotBeEmpty();
            connectivityCheckResponse.Messages.First().Message.ShouldStartWith($"Failed to connect to {baseUrl}.");
        }

        [Test]
        public async Task WhenUsernameIsNotRight()
        {
            var action = new JiraCredentialsConnectivityCheckAction(store, httpClientFactory, log);
            var octoRequest = Substitute.For<IOctoRequest>();
            var baseUrl = store.GetBaseUrl();
            octoRequest.GetBody(Arg.Any<RequestBodyRegistration<JiraCredentialsConnectionCheckData>>())
                       .Returns(new JiraCredentialsConnectionCheckData { BaseUrl = baseUrl, Username = "Does not matter", Password = "Does not matter" });

            var response = await action.ExecuteAsync(octoRequest);

            var connectivityCheckResponse = (ConnectivityCheckResponse)((OctoDataResponse)response.Response).Model;

            connectivityCheckResponse.Messages.ShouldNotBeEmpty();
            connectivityCheckResponse.Messages.First().Message.ShouldStartWith($"Failed to connect to {baseUrl}. Response code: Unauthorized");
        }
    }
}