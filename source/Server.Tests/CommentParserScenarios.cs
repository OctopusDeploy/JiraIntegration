using System.Linq;
using NUnit.Framework;
using Octopus.Server.Extensibility.HostServices.Model.IssueTrackers;
using Octopus.Server.Extensibility.HostServices.Model.PackageMetadata;
using Octopus.Server.Extensibility.IssueTracker.Jira.WorkItems;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Tests
{
    [TestFixture]
    public class CommentParserScenarios
    {
        [Test]
        public void StandardIssueNumberReferenceGetsParsedCorrectly()
        {
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create("Fixes JRE-1234"));
            Assert.IsNotEmpty(workItemReferences);
            
            var reference = workItemReferences.First();
            Assert.AreEqual("JRE-1234", reference);
        }
        
        [Test]
        public void StandardIssueNumberWithAlphaNumericProjectIdentifierReferenceGetsParsedCorrectly()
        {
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create("Fixes BT2-1234"));
            Assert.IsNotEmpty(workItemReferences);
            
            var reference = workItemReferences.First();
            Assert.AreEqual("BT2-1234", reference);
        }

        [Test]
        public void MultipleIssueNumberReferencesGetsParsedCorrectly()
        {
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create("Fixes JRE-1234,JRE-2345"));
            Assert.IsNotEmpty(workItemReferences);
            Assert.AreEqual(2, workItemReferences.Length);
            
            var reference = workItemReferences.First();
            Assert.AreEqual("JRE-1234", reference);
            reference = workItemReferences.Last();
            Assert.AreEqual("JRE-2345", reference);
        }

        [Test]
        public void MultipleIssueNumberWithAlphaNumericProjectIdentifierReferencesGetsParsedCorrectly()
        {
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create("Fixes BT2-1234,BT2-2345"));
            Assert.IsNotEmpty(workItemReferences);
            Assert.AreEqual(2, workItemReferences.Length);
            
            var reference = workItemReferences.First();
            Assert.AreEqual("BT2-1234", reference);
            reference = workItemReferences.Last();
            Assert.AreEqual("BT2-2345", reference);
        }

        [Test]
        public void NonIssueNumberReferenceGetsParsedCorrectly()
        {
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create("Some text -2-bar"));
            Assert.IsEmpty(workItemReferences);
        }
        
        private OctopusPackageMetadata Create(string comment)
        {
            return new OctopusPackageMetadata
            {
                Commits = new[] { new Commit{ Id = "a", Comment = comment }}
            };
        }
    }
}