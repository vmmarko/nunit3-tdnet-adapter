﻿namespace NUnitTDNet.Adapter.Tests.Expect
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestDriven.Framework;
    using Fakes;
    using System.Reflection;
    using Expected;

    [TestClass]
    public class ExpectTests
    {
        [TestMethod]
        public void ExpectAttributeBasedAssertions()
        {
            var sampleType = typeof(NUnitTDNet.Adapter.Examples.Expected.ExpectClass);
            var testAssembly = sampleType.Assembly;
            var explorer = new ExpectAttributeExplorer(testAssembly);
            var testRunner = new EngineTestRunner();
            foreach (var expectEntry in explorer)
            {
                Console.WriteLine(expectEntry.Name);
                ExpectAttributeBasedAssertions(testRunner, expectEntry);
            }
        }

        public void ExpectAttributeBasedAssertions(ITestRunner testRunner, ExpectEntry expectEntry)
        {
            var name = expectEntry.Name;
            var testAssembly = expectEntry.TestAssembly;
            var member = expectEntry.Member;
            var expectAttribute = expectEntry.ExpectAttribute;

            var testListener = new FakeTestListener();
            TestRunState testRunState;

            if (expectAttribute.Namespace)
            {
                var ns = getNamespace(member);
                testRunState = testRunner.RunNamespace(testListener, testAssembly, ns);
            }
            else
            {
                testRunState = testRunner.RunMember(testListener, testAssembly, member);
            }

            if (expectAttribute is ExpectTestRunAttribute)
            {
                // Checks for all tests.
                var expectTestRunAttribute = (ExpectTestRunAttribute)expectAttribute;
                {
                    string message = string.Format("Checking 'TestRunState' for: " + name);
                    Assert.AreEqual(expectTestRunAttribute.TestRunState, testRunState, message);
                }

                if (expectTestRunAttribute.PassedCount >= 0)
                {
                    string message = string.Format("Checking 'PassedCount' for: " + name);
                    Assert.AreEqual(expectTestRunAttribute.PassedCount, testListener.PassedCount, message);
                }

                if (expectTestRunAttribute.IgnoredCount >= 0)
                {
                    string message = string.Format("Checking 'IgnoredCount' for: " + name);
                    Assert.AreEqual(expectTestRunAttribute.IgnoredCount, testListener.IgnoredCount, message);
                }

                if (expectTestRunAttribute.FailedCount >= 0)
                {
                    string message = string.Format("Checking 'FailedCount' for: " + name);
                    Assert.AreEqual(expectTestRunAttribute.FailedCount, testListener.FailedCount, message);
                }
            }

            if (expectAttribute is ExpectTestAttribute)
            {
                // Checks for specific test.
                var expectTestAttribute = (ExpectTestAttribute)expectAttribute;

                string expectName = expectTestAttribute.Name;
                var testResult = testListener.GetTestResult(expectName);
                if (testResult == null)
                {
                    foreach (string testName in testListener.GetTestNames())
                    {
                        Console.WriteLine("found: " + testName);
                    }

                    string message = string.Format("Looking up test with name: " + expectName);
                    Assert.IsNotNull(testResult, message);
                }

                if (expectTestAttribute.Message != null)
                {
                    string message = string.Format("Checking 'Message' for test: " + expectName);
                    Assert.AreEqual(expectTestAttribute.Message, testResult.Message, message);
                }

                if (expectTestAttribute.StackTraceStartsWith != null)
                {
                    string message = string.Format("Checking 'StackTrace' for test: " + expectName);
                    StringAssert.StartsWith(testResult.StackTrace, expectTestAttribute.StackTraceStartsWith, message);
                }

                if (expectTestAttribute.StackTraceEndsWith != null)
                {
                    string message = string.Format("Checking 'StackTrace' for test: " + expectName);
                    StringAssert.EndsWith(testResult.StackTrace, expectTestAttribute.StackTraceEndsWith, message);
                }

                if (expectTestAttribute.TotalTests >= 0)
                {
                    string message = string.Format("Checking 'TotalTests' for test: " + expectName);
                    Assert.AreEqual(expectTestAttribute.TotalTests, testResult.TotalTests, message);
                }

                if (expectTestAttribute.State != null)
                {
                    string message = string.Format("Checking 'State' for test: " + expectName);
                    Assert.AreEqual(expectTestAttribute.State, testResult.State, message);
                }
            }

            if (expectAttribute is ExpectOutputLineAttribute)
            {
                var expectOutputLineAttribute = (ExpectOutputLineAttribute)expectAttribute;

                if (expectOutputLineAttribute.Text == null)
                {
                    Assert.IsTrue(testListener.OutputLines.Count == 0, "Check there is no output.");
                }
                else
                {
                    Assert.IsTrue(testListener.OutputLines.Count > 0, "Output lines were expected.");
                    var outputLineAndCategory = testListener.OutputLines[0];
                    var text = outputLineAndCategory.Item1;
                    var category = outputLineAndCategory.Item2;

                    if (expectOutputLineAttribute.Text != null)
                    {
                        string message = string.Format("Checking output line for test: " + name);
                        Assert.AreEqual(expectOutputLineAttribute.Text, text, message);
                    }

                    if (expectOutputLineAttribute.Category != null)
                    {
                        string message = string.Format("Checking output category for test: " + name);
                        Assert.AreEqual(expectOutputLineAttribute.Category, category, message);
                    }
                }
            }
        }

        private string getNamespace(MemberInfo member)
        {
            var type = member as Type;
            if (type == null)
            {
                Assert.Fail("Namespace must be defined on a Type, not: " + member);
            }

            return type.Namespace;
        }
    }
}
