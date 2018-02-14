using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;

namespace Mufu.SpecFlow.DotNetCoreFix.Tasks
{
    public class BindGeneratedSources : Task
    {
        private static readonly Regex Line1Regex = new Regex(@"^(#line 1 \"")(.*\.feature)(\"")$", RegexOptions.Multiline);

        [Required]
        public ITaskItem[] FeatureFiles { get; set; }

        [Required]
        public ITaskItem[] GeneratedSources { get; set; }

        /// <summary>
        /// How many times to attempt to modify a file, if all previous attempts failed. Defaults to 10.
        /// </summary>
        public int Retries { get; set; } = 10;

        /// <summary>
        /// Delay between any retry attempts. Default value is 500 milliseconds.
        /// </summary>
        public int RetryDelayMilliseconds { get; set; } = 500;

        public BindGeneratedSources()
            : base(AssemblyResources.PrimaryResources)
        {
        }

        private bool ValidateInputs()
        {
            if (Retries < 0)
            {
                Log.LogErrorWithCodeFromResources("BindGeneratedSources.InvalidRetryCount", Retries);
                return false;
            }

            if (RetryDelayMilliseconds < 0)
            {
                Log.LogErrorWithCodeFromResources("BindGeneratedSources.InvalidRetryDelay", RetryDelayMilliseconds);
                return false;
            }

            if (GeneratedSources.Length != FeatureFiles.Length)
            {
                Log.LogErrorWithCodeFromResources(
                    "BindGeneratedSources.TwoVectorsMustHaveSameLength",
                    GeneratedSources.Length,
                    FeatureFiles.Length,
                    nameof(GeneratedSources),
                    nameof(FeatureFiles));
                return false;
            }

            return true;
        }

        public override bool Execute()
        {
            if (!ValidateInputs())
            {
                return false;
            }

            if (GeneratedSources == null || GeneratedSources.Length == 0)
            {
                return true;
            }

            bool success = true;

            for (int i = 0; i < GeneratedSources.Length; i++)
            {
                // In the generate file, replace the #line 1 "<filename>" reference with a link to this source file.
                try
                {
                    success = BindGeneratedFileWithRetries(GeneratedSources[i], FeatureFiles[i]);
                }
                catch (Exception ex) when (IsIoRelatedException(ex))
                {
                    success = false;
                }
            }

            return true;
        }

        private bool BindGeneratedFileWithRetries(ITaskItem generatedFile, ITaskItem sourceFile)
        {
            int retryCount = 0;

            while (true)
            {
                try
                {
                    return BindGeneratedFile(generatedFile, sourceFile);
                }
                catch(Exception ex) when (IsIoRelatedException(ex))
                {
                    if (retryCount < Retries)
                    {
                        retryCount++;
                        Log.LogWarningWithCodeFromResources(
                            "BindGeneratedSources.Retrying",
                            generatedFile.ItemSpec,
                            sourceFile.ItemSpec,
                            retryCount,
                            RetryDelayMilliseconds,
                            ex.Message);

                        Thread.Sleep(RetryDelayMilliseconds);
                    }
                    else if (Retries > 0)
                    {
                        Log.LogErrorWithCodeFromResources(
                            "BindGeneratedSources.ExceededRetries",
                            generatedFile.ItemSpec,
                            sourceFile.ItemSpec,
                            Retries);
                        throw;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private bool BindGeneratedFile(ITaskItem generatedFile, ITaskItem sourceFile)
        {
            var generatedFilePullPath = generatedFile.GetMetadata("FullPath");
            var sourceFileFullPath = sourceFile.GetMetadata("FullPath");

            try
            {
                using (var file = File.Open(generatedFilePullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    var lines = new List<string>();
                    bool replaceFileContents = false;

                    using (var reader = new StreamReader(file))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var replacedLine = Line1Regex.Replace(
                                line,
                                match =>
                                {
                                    replaceFileContents = true;
                                    return string.Join("", match.Groups[1].Value, sourceFileFullPath, match.Groups[3].Value);
                                });

                            lines.Add(replacedLine);
                        }

                        if (replaceFileContents)
                        {
                            file.SetLength(0);

                            using (var writer = new StreamWriter(file))
                            {
                                foreach (var line2 in lines)
                                {
                                    writer.WriteLine(line2);
                                }

                                writer.Flush();
                            }
                        }
                    }
                }

                Log.LogMessageFromResources(
                    MessageImportance.Normal,
                    "BindGeneratedSources.BoundFile",
                    generatedFile.ItemSpec,
                    sourceFile.ItemSpec);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        private bool IsIoRelatedException(Exception ex)
        {
            return ex is UnauthorizedAccessException
                || ex is NotSupportedException
                || (ex is ArgumentException && !(ex is ArgumentNullException))
                || ex is SecurityException
                || ex is IOException;
        }
    }
}
