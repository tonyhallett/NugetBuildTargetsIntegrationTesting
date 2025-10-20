namespace NugetBuildTargetsIntegrationTesting.Builder
{
    internal class ProjectBuildContext(
        string projectContents,
        string projectRelativePath,
        string nuPkgPath,
        IEnumerable<(string Contents, string RelativePath)>? _files)
    {
        public string ProjectContents { get; } = projectContents;

        public string ProjectRelativePath { get; } = projectRelativePath;

        public string NuPkgPath { get; } = nuPkgPath;

        public IEnumerable<(string Contents, string RelativePath)>? Files { get; } = _files;
    }
}
