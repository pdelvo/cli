// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.Tools.Test.Utilities;
using Microsoft.Extensions.PlatformAbstractions;
using NuGet.Frameworks;
using Xunit;

namespace Microsoft.DotNet.Tools.Compiler.Tests
{
    public class CompilerOutputTests : TestBase
    {
        private readonly string _testProjectsRoot;
        private readonly string[] _runtimeFiles = { "TestApp.dll", "TestApp.pdb", "TestApp.exe", "TestApp.deps", "TestLibrary.dll", "TestLibrary.pdb" };
        private readonly string[] _appCompileFiles = { "TestApp.dll", "TestApp.pdb" };
        private readonly string[] _libCompileFiles = { "TestLibrary.dll", "TestLibrary.pdb" };

        public CompilerOutputTests()
        {
            _testProjectsRoot = Path.Combine(AppContext.BaseDirectory, @"TestProjects");
        }

        private void PrepareProject(out TempDirectory root, out TempDirectory testAppDir, out TempDirectory testLibDir, out string runtime)
        {
            root = Temp.CreateDirectory();

            testAppDir = root.CreateDirectory("TestApp");
            testLibDir = root.CreateDirectory("TestLibrary");

            // copy projects to the temp dir
            CopyProjectToTempDir(Path.Combine(_testProjectsRoot, "TestApp"), testAppDir);
            CopyProjectToTempDir(Path.Combine(_testProjectsRoot, "TestLibrary"), testLibDir);

            var contexts = ProjectContext.CreateContextForEachFramework(
                testLibDir.Path,
                null,
                PlatformServices.Default.Runtime.GetAllCandidateRuntimeIdentifiers());
            runtime = contexts.FirstOrDefault(c => !string.IsNullOrEmpty(c.RuntimeIdentifier))?.RuntimeIdentifier;
        }

        [Fact]
        public void DefaultPaths()
        {
            TempDirectory root;
            TempDirectory testAppDir;
            TempDirectory testLibDir;
            string runtime;
            PrepareProject(out root, out testAppDir, out testLibDir, out runtime);

            new BuildCommand(GetProjectPath(testAppDir))
                .ExecuteWithCapturedOutput().Should().Pass();

            var libdebug = testLibDir.DirectoryInfo.Sub("bin").Sub("Debug");
            var appdebug = testLibDir.DirectoryInfo.Sub("bin").Sub("Debug");
            var appruntime = appdebug.Sub(runtime);

            libdebug.Should().Exist().And.HaveFiles(_libCompileFiles);
            appdebug.Should().Exist().And.HaveFiles(_appCompileFiles);
            appruntime.Should().Exist().And.HaveFiles(_runtimeFiles);
        }

        [Fact]
        public void OutputDirSet()
        {
            TempDirectory root;
            TempDirectory testAppDir;
            TempDirectory testLibDir;
            string runtime;
            PrepareProject(out root, out testAppDir, out testLibDir, out runtime);

            var output = root.CreateDirectory("output");

            new BuildCommand(GetProjectPath(testAppDir), output: output.Path)
                .ExecuteWithCapturedOutput().Should().Pass();

            var libdebug = testLibDir.DirectoryInfo.Sub("bin").Sub("Debug");
            var appdebug = testLibDir.DirectoryInfo.Sub("bin").Sub("Debug");

            libdebug.Should().Exist().And.HaveFiles(_libCompileFiles);
            appdebug.Should().Exist().And.HaveFiles(_appCompileFiles);
            output.DirectoryInfo.Should().HaveFiles(_runtimeFiles);
        }

        [Fact]
        public void BuildBaseDirSet()
        {
            TempDirectory root;
            TempDirectory testAppDir;
            TempDirectory testLibDir;
            string runtime;
            PrepareProject(out root, out testAppDir, out testLibDir, out runtime);

            var buildBase = root.CreateDirectory("buildBase");

            new BuildCommand(GetProjectPath(testAppDir), buidBasePath: buildBase.Path)
                .ExecuteWithCapturedOutput().Should().Pass();

            var libdebug = buildBase.DirectoryInfo.Sub("TestLibrary").Sub("bin").Sub("Debug");
            var appdebug = buildBase.DirectoryInfo.Sub("TestApp").Sub("Debug");
            var appruntime = appdebug.Sub(runtime);

            libdebug.Should().Exist().And.HaveFiles(_libCompileFiles);
            appdebug.Should().Exist().And.HaveFiles(_appCompileFiles);
            appruntime.Should().Exist().And.HaveFiles(_runtimeFiles);
        }

        [Fact]
        public void BuildBaseAndOutputDirSet()
        {
            TempDirectory root;
            TempDirectory testAppDir;
            TempDirectory testLibDir;
            string runtime;
            PrepareProject(out root, out testAppDir, out testLibDir, out runtime);

            var output = root.CreateDirectory("output");
            var buildBase = root.CreateDirectory("buildBase");

            new BuildCommand(GetProjectPath(testAppDir), output:output.Path, buidBasePath: buildBase.Path)
                .ExecuteWithCapturedOutput().Should().Pass();

            var libdebug = buildBase.DirectoryInfo.Sub("TestLibrary").Sub("bin").Sub("Debug");
            var appdebug = buildBase.DirectoryInfo.Sub("TestApp").Sub("Debug");

            libdebug.Should().Exist().And.HaveFiles(_libCompileFiles);
            appdebug.Should().Exist().And.HaveFiles(_appCompileFiles);
            output.DirectoryInfo.Should().HaveFiles(_runtimeFiles);
        }


        private void CopyProjectToTempDir(string projectDir, TempDirectory tempDir)
        {
            // copy all the files to temp dir
            foreach (var file in Directory.EnumerateFiles(projectDir))
            {
                tempDir.CopyFile(file);
            }
        }

        private string GetProjectPath(TempDirectory projectDir)
        {
            return Path.Combine(projectDir.Path, "project.json");
        }
    }
}
