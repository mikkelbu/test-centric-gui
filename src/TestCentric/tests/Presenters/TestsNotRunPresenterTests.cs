﻿// ***********************************************************************
// Copyright (c) 2018 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;

namespace TestCentric.Gui.Presenters
{
    using Model;
    using Views;

    public class TestsNotRunPresenterTests
    {
        private ITestsNotRunView _view;
        private ITestModel _model;
        private TestsNotRunPresenter _presenter;

        private static readonly TestNode FAKE_TEST_RUN = new TestNode("<test-suite id='1' testcasecount='1234' />");

        [SetUp]
        public void CreatePresenter()
        {
            _view = Substitute.For<ITestsNotRunView>();
            _model = Substitute.For<ITestModel>();

            _presenter = new TestsNotRunPresenter(_view, _model);
        }

        [Test]
        public void WhenTestIsLoaded_DisplayIsCleared()
        {
            _model.Events.TestLoaded += Raise.Event<TestNodeEventHandler>(new TestNodeEventArgs(FAKE_TEST_RUN));

            _view.Received().Clear();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WhenTestIsReloaded_DisplayIsClearedDependingOnSetting(bool clear)
        {
            var fakeSettings = new FakeUserSettings();
            fakeSettings.Gui.ClearResultsOnReload = clear;

            _model.Services.UserSettings.Returns(fakeSettings);
            _model.Events.TestReloaded += Raise.Event<TestNodeEventHandler>(new TestNodeEventArgs(FAKE_TEST_RUN));

            if (clear)
                _view.Received().Clear();
            else
                _view.DidNotReceive().Clear();
        }

        [Test]
        public void WhenTestIsUnloaded_DisplayIsCleared()
        {
            _model.Events.TestUnloaded += Raise.Event<TestEventHandler>(new TestEventArgs());

            _view.Received().Clear();
        }

        [Test]
        public void WhenTestRunStarts_DisplayIsCleared()
        {
            _model.Events.RunStarting += Raise.Event<RunStartingEventHandler>(new RunStartingEventArgs(1234));

            _view.Received().Clear();
        }

        [TestCase("Skipped", "Test", true)]
        [TestCase("Skipped", "SetUp", true)]
        [TestCase("Skipped", "TearDown", true)]
        [TestCase("Skipped", "Parent", false)]
        [TestCase("Passed", "Test", false)]
        [TestCase("Failed", "Test", false)]
        [TestCase("Warning", "Test", false)]
        [TestCase("Inconclusive", "Test", false)]
        public void TestsCasesAreHandledCorrectly(string status, string site, bool shouldBeAdded)
        {
            var result = new ResultNode($"<test-case id='1' name='NAME' result='{status}' site='{site}'><reason><message>REASON</message></reason></test-case>");
            _model.Events.TestFinished += Raise.Event<TestResultEventHandler>(new TestResultEventArgs(result));

            if (shouldBeAdded)
                _view.Received().AddResult("NAME", "REASON");
            else
                _view.DidNotReceiveWithAnyArgs().AddResult(null, null);
        }

        [TestCase("Skipped", "Test", "REASON", true)]
        [TestCase("Skipped", "SetUp", "REASON", true)]
        [TestCase("Skipped", "TearDown", "REASON", true)]
        [TestCase("Skipped", "Child", "REASON", false)]
        [TestCase("Skipped", "Parent", "REASON", false)]
        [TestCase("Passed", "Test", "REASON", false)]
        [TestCase("Failed", "Test", "REASON", false)]
        [TestCase("Warning", "Test", "REASON", false)]
        [TestCase("Inconclusive", "Test", "REASON", false)]
        [TestCase("Skipped", "Test", "One or more child tests were ignored", false)]
        public void TestSuitesAreHandledCorrectly(string status, string site, string reason, bool shouldBeAdded)
        {
            var result = new ResultNode($"<test-suite id='1' name='NAME' result='{status}' site='{site}'><reason><message>{reason}</message></reason></test-suite>");
            _model.Events.SuiteFinished += Raise.Event<TestResultEventHandler>(new TestResultEventArgs(result));

            if (shouldBeAdded)
                _view.Received().AddResult("NAME", "REASON");
            else
                _view.DidNotReceiveWithAnyArgs().AddResult(null, null);
        }
    }
}
