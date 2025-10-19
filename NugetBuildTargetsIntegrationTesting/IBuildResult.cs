
namespace NugetBuildTargetsIntegrationTesting
{
    public interface IBuildResult
    {
        DirectoryInfo ProjectDirectory { get; }

        public DirectoryInfo ContainingDirectory { get; }
        
        public ProcessResult ProcessResult { get; }
    }
}
