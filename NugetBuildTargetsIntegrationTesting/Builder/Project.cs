namespace NugetBuildTargetsIntegrationTesting.Builder
{
    internal sealed class Project(IBuildManager buildManager) : IProject, IAddNuget, IProjectBuilder
    {
        public const string DefaultProjectRelativePath = "dependentProject.csproj";
        private IEnumerable<(string Contents, string RelativePath)>? _files;
        private string? _projectContents;
        private string? _relativePath;
        private string? _nuPkgPath;

        public IProject AddFiles(IEnumerable<(string Contents, string RelativePath)> files)
        {
            _files = files;
            return this;
        }

        public IAddNuget AddProject(string projectContents) => AddProject(projectContents, DefaultProjectRelativePath);

        public IAddNuget AddProject(string projectContents, string relativePath)
        {
            _projectContents = projectContents;
            _relativePath = relativePath;
            return this;
        }

        public IProjectBuilder AddNuPkg(string nuPkgPath)
        {
            _nuPkgPath = nuPkgPath;
            return this;
        }

        public Task<IBuildResult> BuildWithDotNetAsync(string arguments = "") => BuildProject(true, arguments);

        public Task<IBuildResult> BuildWithMSBuildAsync(string arguments = "") => BuildProject(false, arguments);

        private async Task<IBuildResult> BuildProject(bool isDotNet, string arguments)
        {
            BuildResult result = await buildManager.BuildAsync(
                new ProjectBuildContext(_projectContents!, _relativePath!, _nuPkgPath!, _files),
                isDotNet,
                arguments);
            return result;
        }
    }
}
