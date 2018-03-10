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

#if NET_3_5 || NET_4_0 || NET_4_5
using System;
using System.IO;
using NSubstitute;
using NUnit.Framework;

namespace NUnit.ProjectEditor.Tests.Presenters
{
    [TestFixture, Platform("Net-3.5,Mono-3.5,Net-4.0")]
    public class AddConfigurationPresenterTests
    {
        IProjectModel model;
        IAddConfigurationDialog dlg;
        AddConfigurationPresenter presenter;

        [SetUp]
        public void SetUp()
        {
            IProjectDocument doc = new ProjectDocument();
            doc.LoadXml(NUnitProjectXml.NormalProject);
            model = new ProjectModel(doc);

            dlg = Substitute.For<IAddConfigurationDialog>();

            presenter = new AddConfigurationPresenter(model, dlg);
        }

        [Test]
        public void ConfigList_LoadFromModel_SetsViewCorrectly()
        {
            Assert.That(dlg.ConfigList, Is.EqualTo(new string[] {"Debug", "Release"}));
        }

        [Test]
        public void AddButton_AddNewConfig_IsAddedToList()
        {
            dlg.ConfigToCreate.Returns("New");
            dlg.OkButton.Execute += Raise.Event<CommandDelegate>();

            Assert.That(model.ConfigNames, Is.EqualTo(new string[] {"Debug", "Release", "New"}));
        }

        [Test]
        public void AddButton_AddExistingConfig_FailsWithErrorMessage()
        {
            dlg.ConfigToCreate.Returns("Release");
            dlg.OkButton.Execute += Raise.Event<CommandDelegate>();

            dlg.MessageDisplay.Received().Error("A configuration with that name already exists");
            Assert.That(model.ConfigNames, Is.EqualTo(new string[] { "Debug", "Release" }));
        }

        [Test]
        public void ConfigToCopy_WhenNotSpecified_ConfigIsEmpty()
        {
            dlg.ConfigToCreate.Returns("New");
            dlg.ConfigToCopy.Returns("<none>");

            dlg.OkButton.Execute += Raise.Event<CommandDelegate>();

            Assert.That(model.ConfigNames, Is.EqualTo(new string[] { "Debug", "Release", "New" }));
            Assert.That(model.Configs[2].BasePath, Is.EqualTo(null));
            Assert.That(model.Configs[2].Assemblies.Count, Is.EqualTo(0));
        }

        [Test]
        public void ConfigToCopy_WhenSpecified_ConfigIsCopied()
        {
            dlg.ConfigToCreate.Returns("New");
            dlg.ConfigToCopy.Returns("Release");

            dlg.OkButton.Execute += Raise.Event<CommandDelegate>();

            Assert.That(model.ConfigNames, Is.EqualTo(new string[] { "Debug", "Release", "New" }));
            Assert.That(model.Configs[2].BasePath, Is.EqualTo("bin" + Path.DirectorySeparatorChar + "release"));
            Assert.That(model.Configs[2].Assemblies.Count, Is.EqualTo(2));
        }
    }
}
#endif