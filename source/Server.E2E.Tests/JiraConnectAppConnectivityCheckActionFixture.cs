using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Web.Api;
using Octopus.Server.Extensibility.HostServices.Licensing;
using Octopus.Server.Extensibility.JiraIntegration.Integration;
using Octopus.Server.Extensibility.JiraIntegration.Web;
using Octopus.Server.Extensibility.Resources.Configuration;
using Shouldly;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.Models;

namespace Octopus.Server.Extensibility.JiraIntegration.E2E.Tests
{
    [TestFixture]
    internal class JiraConnectAppConnectivityCheckActionFixture : ConnectivityCheckActionsBaseFixture
    {
        [Test]
        public async Task WhenDnsCannotBeResolved()
        {
            var installationIdProvider = Substitute.For<IInstallationIdProvider>();
            installationIdProvider.GetInstallationId().Returns(Guid.NewGuid());

            var baseUrl = "http://notexistingdomain.dddd.ttt";
            store.GetConnectAppUrl().Returns(baseUrl);

            var action = new JiraConnectAppConnectivityCheckAction(log, store, installationIdProvider,
                new JiraConnectAppClient(installationIdProvider, store, httpClientFactory), httpClientFactory);
            var octoRequest = Substitute.For<IOctoRequest>();
            octoRequest.GetBody(Arg.Any<RequestBodyRegistration<JiraConnectAppConnectionCheckData>>())
                .Returns(new JiraConnectAppConnectionCheckData
                    { BaseUrl = baseUrl, Password = "Does not matter" });

            var response = await action.ExecuteAsync(octoRequest);

            var connectivityCheckResponse = (ConnectivityCheckResponse)((OctoDataResponse)response.Response).Model;

            connectivityCheckResponse.Messages.ShouldNotBeEmpty();
            connectivityCheckResponse.Messages.First().Message
                .ShouldBe("Failed to get authentication token from Jira Connect App.");
        }

        [Test]
        public async Task WhenProxyAuthenticationIsRequired()
        {
            using var proxyServer = new ProxyServer();
            const int port = 12312;

            var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, port, false);
            proxyServer.AddEndPoint(explicitEndPoint);
            // Fake authentication failure for proxy
            proxyServer.ProxySchemeAuthenticateFunc = (d, s, arg3) => Task.FromResult(new ProxyAuthenticationContext
                { Result = ProxyAuthenticationResult.Failure });
            proxyServer.ProxyAuthenticationSchemes = new[] { "basic" };
            proxyServer.Start();
            var baseUrl = "http://notexistingdomain.dddd.ttt";

            httpClientFactory.CreateClient().Returns(_ =>
            {
                var httpClient = new HttpClient(new HttpClientHandler { Proxy = new WebProxy("127.0.0.1", port) })
                {
                    BaseAddress = new Uri(baseUrl)
                };

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes("username:authToken")));

                return httpClient;
            });

            var installationIdProvider = Substitute.For<IInstallationIdProvider>();
            installationIdProvider.GetInstallationId().Returns(Guid.NewGuid());

            store.GetConnectAppUrl().Returns(baseUrl);

            var action = new JiraConnectAppConnectivityCheckAction(log, store, installationIdProvider,
                new JiraConnectAppClient(installationIdProvider, store, httpClientFactory), httpClientFactory);
            var octoRequest = Substitute.For<IOctoRequest>();
            octoRequest.GetBody(Arg.Any<RequestBodyRegistration<JiraConnectAppConnectionCheckData>>())
                .Returns(new JiraConnectAppConnectionCheckData
                    { BaseUrl = baseUrl, Password = "Does not matter" });

            var response = await action.ExecuteAsync(octoRequest);

            var connectivityCheckResponse = (ConnectivityCheckResponse)((OctoDataResponse)response.Response).Model;

            connectivityCheckResponse.Messages.ShouldNotBeEmpty();
            connectivityCheckResponse.Messages.First().Message
                .ShouldBe("Failed to get authentication token from Jira Connect App.");
        }
    }
}