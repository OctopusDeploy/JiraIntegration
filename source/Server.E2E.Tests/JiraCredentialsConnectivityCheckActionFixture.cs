using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Octopus.Server.Extensibility.JiraIntegration.Web;
using Shouldly;

namespace Octopus.Server.Extensibility.JiraIntegration.E2E.Tests
{
    [TestFixture]
    internal class JiraCredentialsConnectivityCheckActionFixture : ConnectivityCheckActionsBaseFixture
    {
        [Test]
        public async Task WhenDnsCannotBeResolved()
        {
            var action = new JiraCredentialsConnectivityCheckAction(store, httpClientFactory, log);
            var baseUrl = "http://notexistingdomain.dddd.ttt/";
            var connectivityCheckResponse = await action.Execute(
                new JiraCredentialsConnectionCheckData
                    { BaseUrl = baseUrl, Username = "Does not matter", Password = "Does not matter" },
                CancellationToken.None);

            connectivityCheckResponse.Messages.ShouldNotBeEmpty();
            connectivityCheckResponse.Messages.First().Message.ShouldStartWith($"Failed to connect to {baseUrl}.");
        }

        [Test]
        public async Task WhenUsernameIsNotRight()
        {
            var action = new JiraCredentialsConnectivityCheckAction(store, httpClientFactory, log);
            var baseUrl = new Uri(await store.GetBaseUrl(CancellationToken.None)).ToString();
            var connectivityCheckResponse = await action.Execute(
                new JiraCredentialsConnectionCheckData
                    { BaseUrl = baseUrl, Username = "Does not matter", Password = "Does not matter" },
                CancellationToken.None);

            connectivityCheckResponse.Messages.ShouldNotBeEmpty();
            connectivityCheckResponse.Messages.First().Message
                .ShouldStartWith($"Failed to connect to {baseUrl}. Response code: Unauthorized");
        }
    }
}