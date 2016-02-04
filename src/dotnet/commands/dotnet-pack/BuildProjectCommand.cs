﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Text;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using System.Collections.Generic;

namespace Microsoft.DotNet.Tools.Pack
{
    internal class BuildProjectCommand
    {
        private readonly Project _project;
        private readonly ArtifactPathsCalculator _artifactPathsCalculator;

        private readonly string _buildBasePath;
        private readonly string _configuration;

        private bool SkipBuild => _artifactPathsCalculator.CompiledArtifactsPathSet;
        
        public BuildProjectCommand(
            Project project, 
            ArtifactPathsCalculator artifactPathsCalculator, 
            string buildBasePath, 
            string configuration)
        {
            _project = project;
            _artifactPathsCalculator = artifactPathsCalculator;
            _buildBasePath = buildBasePath;
            _configuration = configuration;
        }

        public int Execute()
        {
            if (SkipBuild)
            {
                return 0;
            }

            if (_project.Files.SourceFiles.Any())
            {
                var argsBuilder = new List<string>();
                argsBuilder.Add("--configuration");
                argsBuilder.Add($"{_configuration}");

                if (_artifactPathsCalculator.PackageOutputPathSet)
                {
                    argsBuilder.Add("--output");
                    argsBuilder.Add($"{_artifactPathsCalculator.PackageOutputPathParameter}");
                }

                if (!string.IsNullOrEmpty(_buildBasePath))
                {
                    argsBuilder.Add("--build-base-path");
                    argsBuilder.Add($"{_buildBasePath}");
                }

                argsBuilder.Add($"{_project.ProjectFilePath}");

                var result = Command.CreateDotNet("build", argsBuilder)
                       .ForwardStdOut()
                       .ForwardStdErr()
                       .Execute();

                return result.ExitCode;
            }

            return 0;
        }
    }
}
