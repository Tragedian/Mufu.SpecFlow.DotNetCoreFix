﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Rhubarb.SpecFlow.NetCore.Tasks
{ 
    public class SpecFlowGenerateSources : ToolTask
    {
        private static readonly Regex ExceptionRegex = new Regex(".*\\..*Exception: (?<message>.*)( -->)?");

        private static readonly Regex ErrorRegex = new Regex("Error file (?<file>.*)");

        private static readonly Regex ErrorLineRegex = new Regex(@"Line \d+:\d+ - \((?<line>\d+):(?<column>\d+)\): (?<message>.*)");

        private bool _exceptionHasBeenThrown;

        private string _errorFile;

        public SpecFlowGenerateSources()
            : base(AssemblyResources.PrimaryResources)
        {
        }

        public bool RegenerateAll { get; set; }

        public bool VerboseOutput { get; set; }

        public ITaskItem ProjectFile { get; set; }

        protected override string ToolName { get; } = "SpecFlow.exe";

        protected override bool ValidateParameters()
        {
            if (!base.ValidateParameters())
            {
                return false;
            }

            if (ToolPath == null)
            {
                Log.LogErrorWithCodeFromResources(nameof(SpecFlowGenerateSources) + ".NeedsToolPath", nameof(ToolPath));
                return false;
            }

            if (ProjectFile == null)
            {
                Log.LogErrorWithCodeFromResources(nameof(SpecFlowGenerateSources) + ".NeedsProjectFile", nameof(ProjectFile));
                return false;
            }

            return true;
        }

        protected override string GetWorkingDirectory()
        {
            if (ProjectFile != null)
            {
                return ProjectFile.GetRootDir() + ProjectFile.GetDirectory();
            }

            return base.GetWorkingDirectory();
        }

        protected override string GenerateFullPathToTool()
        {
            var result = Path.Combine(ToolPath, ToolName);

            if (!File.Exists(result))
            {
                return null;
            }

            return result;
        }

        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();

            builder.AppendTextUnquoted(base.GenerateCommandLineCommands());

            builder.AppendSwitch("GenerateAll");

            builder.AppendSwitch("-p");
            builder.AppendSwitch(ProjectFile.GetFullPath());

            if (RegenerateAll)
            {
                builder.AppendSwitch("-f");
            }

            if (VerboseOutput)
            {
                builder.AppendSwitch("-v");
            }

            return builder.ToString();
        }

        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            if (!_exceptionHasBeenThrown)
            {
                var exceptionMatch = ExceptionRegex.Match(singleLine);

                if (exceptionMatch.Success)
                {
                    Log.LogError(exceptionMatch.Groups["message"].Value);
                    _exceptionHasBeenThrown = true;
                    return;
                }

                var errorMatch = ErrorRegex.Match(singleLine);

                if (errorMatch.Success)
                {
                    _errorFile = errorMatch.Groups["file"].Value;
                    return;
                }
            }

            if (_errorFile != null)
            {
                var errorMatch = ErrorLineRegex.Match(singleLine);

                if (errorMatch.Success)
                {
                    var line = int.Parse(errorMatch.Groups["line"].Value);
                    var column = int.Parse(errorMatch.Groups["column"].Value);
                    var message = errorMatch.Groups["message"].Value;

                    Log.LogError("SpecFlow", null, null, _errorFile, line, column, 0, 0, message);

                    _errorFile = null;

                    return;
                }
            }

            if (!_exceptionHasBeenThrown)
            {
                base.LogEventsFromTextOutput(singleLine, messageImportance);
            }
        }
    }
}
