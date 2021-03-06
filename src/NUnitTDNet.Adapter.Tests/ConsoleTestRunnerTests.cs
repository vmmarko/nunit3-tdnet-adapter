﻿namespace NUnitTDNet.Adapter.Tests
{
    using System;
    using System.Reflection;
    using Fakes;
    using Examples;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Threading;
    using System.IO;
    using TestDriven.Framework;

    [TestClass]
    public class ConsoleTestRunnerTests    
    {
        [TestMethod]
        public void RunMember_SomeTestsPass_PassedCount1()
        {
            var testListener = new FakeTestListener();
            var testMethod = new ThreadStart(SomeTests.Pass).Method;
            var testAssembly = testMethod.DeclaringType.Assembly;
            var testRunner = createTestRunner();

            testRunner.RunMember(testListener, testAssembly, testMethod);

            Assert.AreEqual(1, testListener.PassedCount, "Check one test passed.");
        }

        [TestMethod]
        public void RunMember_SomeTestCasesPassAndFail_PassAndFail()
        {
            var testListener = new FakeTestListener();
            var testMethod = new ThreadStart(SomeTests.Fail).Method;
            var testAssembly = testMethod.DeclaringType.Assembly;

            var testRunner = createTestRunner();
            testRunner.RunMember(testListener, testAssembly, testMethod);

            Assert.AreEqual(1, testListener.FailedCount, "Check one test failed.");
        }

        ITestRunner createTestRunner()
        {
            return new ConsoleTestRunner(findDir());
        }

        static string findDir()
        {
            var solutionPath = Environment.GetEnvironmentVariable("NCrunch.OriginalSolutionPath");
            if (solutionPath != null)
            {
                return Path.GetDirectoryName(solutionPath);
            }

            var assembly = Assembly.GetExecutingAssembly();
            var localPath = new Uri(assembly.EscapedCodeBase).LocalPath;
            return Path.GetDirectoryName(localPath);
        }
    }
}
