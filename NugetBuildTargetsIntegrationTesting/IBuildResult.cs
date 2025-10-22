namespace NugetBuildTargetsIntegrationTesting
{
    public interface IBuildResult
    {
        DirectoryInfo ContainingDirectory { get; }

        DirectoryInfo ProjectDirectory { get; }

        string StandardError { get; }

        string ErrorAndOutput { get; }

        string StandardOutput { get; }

        bool Passed { get; }

        IBuildResult AddFiles(IEnumerable<(string Contents, string RelativePath)> files);

        void Rebuild(string? args = null);
    }
}
