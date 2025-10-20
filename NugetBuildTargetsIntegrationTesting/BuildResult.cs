using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting
{
    internal class BuildResult(
            DirectoryInfo projectDirectory,
            DirectoryInfo containingDirectory,
            ProcessResult processResult,
            Action<IEnumerable<(string Contents, string RelativePath)>> addFiles,
            Func<string?,ProcessResult> rebuild
            ) : IBuildResult
    {
        public DirectoryInfo ProjectDirectory { get; } = projectDirectory;

        public DirectoryInfo ContainingDirectory { get; } = containingDirectory;

        private ProcessResult ProcessResult { get; set; } = processResult;

        public bool Passed => ProcessResult.ExitCode == 0;

        public string Output => ProcessResult.Output;

        public string Error => ProcessResult.Error;

        public string ErrorAndOutput => Error + Environment.NewLine + Output;

        public IBuildResult AddFiles(IEnumerable<(string Contents, string RelativePath)> files)
        {
            addFiles(files);
            return this;
        }

        public void Rebuild(string? args = null)
        {
            ProcessResult = rebuild(args);
        }

    }
}
