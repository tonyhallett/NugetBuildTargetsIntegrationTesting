
namespace NugetBuildTargetsIntegrationTesting
{
    internal class Project : IProject, IAddNuget, IBuilder
    {
        private List<(string Contents, string RelativePath)>? _files;
        private string? _projectContents;
        private string? _relativePath;
        private string? _nuPkgPath;
        private readonly IBuildManager _buildManager;

        public Project(IBuildManager doBuild) => this._buildManager = doBuild;

        public IProject AddFiles(List<(string Contents, string RelativePath)> files)
        {
            _files = files;
            return this;
        }

        public IAddNuget AddProject(string projectContents, string relativePath = "dependentProject.csproj")
        {
            _projectContents = projectContents;
            _relativePath = relativePath;
            return this;
        }

        public IBuilder AddNuPkg(string nuPkgPath)
        {
            _nuPkgPath = nuPkgPath;
            return this;
        }


        public IBuildResult DotNetBuildProject(string arguments = "") => BuildProject(true, arguments);


        public IBuildResult MSBuildBuildProject(string arguments = "") => BuildProject(false, arguments);


        private IBuildResult BuildProject(bool isDotNet,string arguments) 
            => _buildManager.Build(
                new ProjectBuildContext(_projectContents!, _relativePath!, _nuPkgPath!, _files), 
                isDotNet, 
                arguments);
    }
}
