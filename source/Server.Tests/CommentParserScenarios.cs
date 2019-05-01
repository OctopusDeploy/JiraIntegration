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
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create("Fixes Bt2-1234,Bt2-2345"));
            Assert.IsNotEmpty(workItemReferences);
            Assert.AreEqual(2, workItemReferences.Length);
            
            var reference = workItemReferences.First();
            Assert.AreEqual("Bt2-1234", reference);
            reference = workItemReferences.Last();
            Assert.AreEqual("Bt2-2345", reference);
        }

        [TestCase("Test with BT2-1234", 1)]
        [TestCase("BT2-1234", 1)]
        [TestCase(" BT2-1234", 1)]
        [TestCase("BT2-1234 ", 1)]
        [TestCase(" BT2-1234 ", 1)]
        [TestCase("Fixes BT2-1234,BT2-2345", 2)]
        [TestCase("Fixes BT2-1234, BT2-2345", 2)]
        [TestCase("Fixes BT2-1234. And include text that may cause confusion test.TST-01.com", 1)]
        public void CommentsGetParsedCorrectly(string comment, int expectedNumber)
        {
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create(comment));
            Assert.AreEqual(expectedNumber, workItemReferences.Length);
        }
        
        [TestCase("Some text -2-bar")]
        [TestCase("Test with test.TST-01.com")]
        [TestCase("Some text test-foo-2")]
        [TestCase("Some text test-foo-2-bar")]
        [TestCase("Some text test-foo-")]
        public void CommentsWithStringThatLookCloseToReferencesGetParsedCorrectly(string comment)
        {
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create(comment));
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