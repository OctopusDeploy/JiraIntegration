using System.Linq;
using NUnit.Framework;
using Octopus.Server.Extensibility.JiraIntegration.WorkItems;
using Octopus.Server.MessageContracts.Features.BuildInformation;

namespace Octopus.Server.Extensibility.JiraIntegration.Tests
{
    [TestFixture]
    internal class CommentParserScenarios
    {
        [Test]
        public void StandardIssueNumberReferenceGetsParsedCorrectly()
        {
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create("Fixes JRE-1234"));
            Assert.IsNotEmpty(workItemReferences);

            var reference = workItemReferences.First();
            Assert.AreEqual("JRE-1234", reference);
        }

        [TestCase("Merge branch 'feature/JRE-1234-Product_Feature", "JRE-1234")]
        public void JiraIssueInBranchNameGetsParsedCorrectly(string comment, string expectedReference)
        {
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create(comment));
            Assert.IsNotEmpty(workItemReferences);

            var reference = workItemReferences.First();
            Assert.AreEqual(expectedReference, reference);
        }

        [TestCase("Fixes [JRE-1234]", "JRE-1234")]
        [TestCase("Fixes (JRE-1234)", "JRE-1234")]
        [TestCase("Fixes (JRE-1234]", "JRE-1234")]
        [TestCase("Fixes [JRE-1234)", "JRE-1234")]
        public void StandardIssueNumberReferenceEnclosedInBracketsGetsParsedCorrectly(string comment,
            string expectedReference)
        {
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create(comment));
            Assert.IsNotEmpty(workItemReferences);

            var reference = workItemReferences.First();
            Assert.AreEqual(expectedReference, reference);
        }

        [Test]
        public void StandardIssueNumberWithAlphaNumericProjectIdentifierReferenceGetsParsedCorrectly()
        {
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create("Fixes BT2-1234"));
            Assert.IsNotEmpty(workItemReferences);

            var reference = workItemReferences.First();
            Assert.AreEqual("BT2-1234", reference);
        }

        [TestCase("Fixes [BT2-1234]", "BT2-1234")]
        [TestCase("Fixes (BT2-1234)", "BT2-1234")]
        public void StandardIssueNumberWithAlphaNumericProjectIdentifierEnclosedInBracketsReferenceGetsParsedCorrectly(
            string comment, string expectedReference)
        {
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create(comment));
            Assert.IsNotEmpty(workItemReferences);

            var reference = workItemReferences.First();
            Assert.AreEqual(expectedReference, reference);
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

        [TestCase("Fixes [JRE-1234],[JRE-2345]", "JRE-1234", "JRE-2345")]
        [TestCase("Fixes (JRE-1234),(JRE-2345)", "JRE-1234", "JRE-2345")]
        public void MultipleIssueNumberReferencesEnclosedInBracketsGetsParsedCorrectly(string comment,
            params string[] expectedReferences)
        {
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create(comment));
            Assert.IsNotEmpty(workItemReferences);
            Assert.AreEqual(2, workItemReferences.Length);

            var reference = workItemReferences.First();
            Assert.AreEqual(expectedReferences[0], reference);
            reference = workItemReferences.Last();
            Assert.AreEqual(expectedReferences[1], reference);
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

        [TestCase("Fixes Bt2-1234,Bt2-2345", "Bt2-1234", "Bt2-2345")]
        public void MultipleIssueNumberWithAlphaNumericProjectIdentifierEnclosedInBracketsReferencesGetsParsedCorrectly(
            string comment, params string[] expectedReferences)
        {
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create("Fixes Bt2-1234,Bt2-2345"));
            Assert.IsNotEmpty(workItemReferences);
            Assert.AreEqual(2, workItemReferences.Length);

            var reference = workItemReferences.First();
            Assert.AreEqual(expectedReferences[0], reference);
            reference = workItemReferences.Last();
            Assert.AreEqual(expectedReferences[1], reference);
        }

        [TestCase("Test with BT2-1234", 1)]
        [TestCase("BT2-1234", 1)]
        [TestCase(" BT2-1234", 1)]
        [TestCase("BT2-1234 ", 1)]
        [TestCase(" BT2-1234 ", 1)]
        [TestCase("Fixes BT2-1234,BT2-2345", 2)]
        [TestCase("Fixes BT2-1234, BT2-2345", 2)]
        [TestCase("Fixes BT2-1234. And include text that may cause confusion test.TST-01.com", 1)]
        [TestCase("[BT2-1234] Implementation", 1)]
        [TestCase("(BT2-1234) Implementation", 1)]
        [TestCase("[BT2-1234] Implementation. And include text that may cause confusion test.TST-01.com", 1)]
        [TestCase("(BT2-1234) Implementation. And include text that may cause confusion test.TST-01.com", 1)]
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
        [TestCase("Something $foo-1")]
        [TestCase("Ignore refs followed by alpha chars Feature-0day")]
        // Due to relaxing the RegEx used to parse issues from comments this test case is no longer valid,
        // it's handled by us checking with the Jira instance if the issue exists, or if it doesnt exist
        // it doesn't get included in the list of work items returned to the UI
        // See https://github.com/OctopusDeploy/JiraIntegration/issues/12 for more context
        // [TestCase("Merge branch 'master' of http://tst-01.com")]
        public void CommentsWithStringThatLookCloseToReferencesGetParsedCorrectly(string comment)
        {
            var workItemReferences = new CommentParser().ParseWorkItemIds(Create(comment));
            Assert.IsEmpty(workItemReferences);
        }

        private OctopusBuildInformation Create(string comment)
        {
            return new OctopusBuildInformation
            {
                Commits = new[] { new Commit { Id = "a", Comment = comment } }
            };
        }
    }
}