using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting
{
    internal sealed class BuildResult(
            DirectoryInfo projectDirectory,
            DirectoryInfo containingDirectory,
            ProcessResult processResult,
            Action<IEnumerable<(string Contents, string RelativePath)>> addFiles,
            Func<string?, Task<ProcessResult>> rebuild) : IBuildResult
    {
        public DirectoryInfo ProjectDirectory { get; } = projectDirectory;

        public DirectoryInfo ContainingDirectory { get; } = containingDirectory;

        private ProcessResult ProcessResult { get; set; } = processResult;

        public bool Passed => ProcessResult.ExitCode == 0;

        public string StandardOutput => ProcessResult.StandardOutput;

        public string StandardError => ProcessResult.StandardError;

        public string ErrorAndOutput => StandardError + Environment.NewLine + StandardOutput;

        public IBuildResult AddFiles(IEnumerable<(string Contents, string RelativePath)> files)
        {
            addFiles(files);
            return this;
        }

        public async Task RebuildAsync(string? args = null) => ProcessResult = await rebuild(args);
    }
}
