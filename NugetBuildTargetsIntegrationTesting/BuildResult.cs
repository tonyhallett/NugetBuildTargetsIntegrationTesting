
namespace NugetBuildTargetsIntegrationTesting
{
    internal class BuildResult : IBuildResult
    {
        public BuildResult(DirectoryInfo projectDirectory, DirectoryInfo containingDirectory, ProcessResult processResult)
        {
            ProjectDirectory = projectDirectory;
            ContainingDirectory = containingDirectory;
            ProcessResult = processResult;
        }

        public DirectoryInfo ProjectDirectory { get; }
        
        public DirectoryInfo ContainingDirectory { get; }

        public ProcessResult ProcessResult { get; }
    }
}
