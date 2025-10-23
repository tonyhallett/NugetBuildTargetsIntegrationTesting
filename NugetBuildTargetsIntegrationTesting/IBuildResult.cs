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

        Task RebuildAsync(string? args = null);
    }
}
