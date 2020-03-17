using System.Linq;
using Assent;
using NUnit.Framework;

namespace Octopus.Server.Extensibility.JiraIntegration.Tests.PublicSurfaceArea
{
    [TestFixture]
    public class PublicSurfaceAreaScenario
    {
        [Test]
        public void ExtensionsShouldKeepThingsPrivate()
        {
            var types = typeof(JiraIssueTrackerExtension).Assembly.GetExportedTypes().Select(t => t.FullName);
            
            this.Assent(string.Join('\n', types));
        }
    }
}