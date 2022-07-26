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
        [TestCase("ftp://notexistingdomain.dddd.ttt")]
        [TestCase("file://notexistingdomain.dddd.ttt")]
        [TestCase("gopher://notexistingdomain.dddd.ttt")]
        [TestCase("http://notexistingdomain.dddd.ttt/#")]
        public async Task WhenInvalidUrlIsUsed(string baseUrl)
        {
            var action = new JiraCredentialsConnectivityCheckAction(store, httpClientFactory, log);
            var octoRequest = Substitute.For<IOctoRequest>();
            octoRequest.GetBody(Arg.Any<RequestBodyRegistration<JiraCredentialsConnectionCheckData>>())
                .Returns(new JiraCredentialsConnectionCheckData { BaseUrl = baseUrl, Username = "Does not matter", Password = "Does not matter" });

            var response = await action.ExecuteAsync(octoRequest);

            var connectivityCheckResponse = (ConnectivityCheckResponse)((OctoDataResponse)response.Response).Model!;

            connectivityCheckResponse.Messages.ShouldNotBeEmpty();
            connectivityCheckResponse.Messages.First().Category.ShouldBe(ConnectivityCheckMessageCategory.Error);
            connectivityCheckResponse.Messages.First().Message.ShouldBe("Invalid data received.");
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
