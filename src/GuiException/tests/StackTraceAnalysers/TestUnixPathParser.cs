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

using NUnit.Framework;
using NUnit.UiException.StackTraceAnalyzers;
using NUnit.UiException.StackTraceAnalysers;

namespace NUnit.UiException.Tests.StackTraceAnalyzers
{
    [TestFixture]
    public class TestUnixPathParser :
        TestIErrorParser
    {
        private IErrorParser _parser;

        [SetUp]
        public new void SetUp()
        {
            _parser = new PathCompositeParser().UnixPathParser;

            Assert.That(_parser, Is.Not.Null);

            return;
        }        

        [Test]
        public void Test_Ability_To_Parse_Regular_Unix_Like_Path_Values()
        {
            RawError res;

            // one basic sample
            res = AcceptValue(_parser, "à NUnit.UiException.TraceItem.get_Text() dans /home/ihottier/TraceItem.cs:ligne 43");
            Assert.That(res.Path, Is.EqualTo("/home/ihottier/TraceItem.cs"));

            // check parser is not confused by file with odd extensions
            res = AcceptValue(_parser, "à get_Text() dans /home/ihottier/TraceItem.cs.cs.cs.cs:ligne 43");
            Assert.That(res.Path, Is.EqualTo("/home/ihottier/TraceItem.cs.cs.cs.cs"));

            // check it supports white space in filePath
            res = AcceptValue(_parser, "à get_Text() dans /home/ihottier/my Document1/my document2 containing space/file.cs:line 1");
            Assert.That(res.Path, Is.EqualTo("/home/ihottier/my Document1/my document2 containing space/file.cs"));

            // check it supports odd folder names ending like C# file
            res = AcceptValue(_parser, "à get_Text() dans /home/my doc/my doc2.cs/file.cs:line 1");
            Assert.That(res.Path, Is.EqualTo("/home/my doc/my doc2.cs/file.cs"));

            // check it doesn't rely on a constant such as "/home"
            res = AcceptValue(_parser, "à get_Text() dans /root/work/folder 1/file1.cs:line 1");
            Assert.That(res.Path, Is.EqualTo("/root/work/folder 1/file1.cs"));

            // same with upper case
            res = AcceptValue(_parser, "à get_Text() dans /ROOT/work/folder 1/file1.cs:line 1");
            Assert.That(res.Path, Is.EqualTo("/ROOT/work/folder 1/file1.cs"));

            // check it doesn't rely upon a specific file language

            res = AcceptValue(_parser, "à get_Text() dans /home/ihottier/work/folder 1/file1.vb:line 1");
            Assert.That(res.Path, Is.EqualTo("/home/ihottier/work/folder 1/file1.vb"));

            res = AcceptValue(_parser, "à get_Text() dans /home/ihottier/work/folder 1/file1.cpp: line 1");
            Assert.That(res.Path, Is.EqualTo("/home/ihottier/work/folder 1/file1.cpp"));

            // check it doesn't rely upon language at all

            res = AcceptValue(_parser, "à get_Text() dans /home/work/folder 1/file1:line 1");
            Assert.That(res.Path, Is.EqualTo("/home/work/folder 1/file1"));

            // check it doesn't rely upon method or line information
            res = AcceptValue(_parser, "/home/work/folder/file:");
            Assert.That(res.Path, Is.EqualTo("/home/work/folder/file"));

            return;
        }

        [Test]
        public void Test_Inability_To_Parse_Non_Unix_Like_Path_Values()
        {
            // check it fails to parse Windows like filePath values
            RejectValue(_parser, "à get_Text() dans C:\\Users\\ihottier\\Work\\file1:line1");

            // check it fails to parse ill-formed Unix filePath values
            RejectValue(_parser, "à get_Text() dans /:line1");
            RejectValue(_parser, "à get_Text() dans / :line1");
            RejectValue(_parser, "à get_Text() dans home/file1:line1");

            // check it fails to parse missing filePath value
            RejectValue(_parser, "à get_Text()");

            return;
        }
    }
}
