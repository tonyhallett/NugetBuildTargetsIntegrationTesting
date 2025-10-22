namespace NugetBuildTargetsIntegrationTesting.Builder
{
    internal sealed class ProjectBuildContext(
        string projectContents,
        string projectRelativePath,
        string nuPkgPath,
        IEnumerable<(string Contents, string RelativePath)>? files)
    {
        public string ProjectContents { get; } = projectContents;

        public string ProjectRelativePath { get; } = projectRelativePath;

        public string NuPkgPath { get; } = nuPkgPath;

        public IEnumerable<(string Contents, string RelativePath)>? Files { get; } = files;
    }
}
