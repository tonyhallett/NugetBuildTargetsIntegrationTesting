namespace NugetBuildTargetsIntegrationTesting
{
    public interface IBuildResult
    {
        DirectoryInfo ContainingDirectory { get; }
        string Error { get; }
        string ErrorAndOutput { get; }
        string Output { get; }
        bool Passed { get; }
        DirectoryInfo ProjectDirectory { get; }

        IBuildResult AddFiles(IEnumerable<(string Contents, string RelativePath)> files);
        void Rebuild(string? args = null);
    }
}
