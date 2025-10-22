using NugetBuildTargetsIntegrationTesting.Builder;
using NugetBuildTargetsIntegrationTesting.Building;
using NugetBuildTargetsIntegrationTesting.IO;
using NugetBuildTargetsIntegrationTesting.MSBuildHelpers;
using NugetBuildTargetsIntegrationTesting.Nuget;
using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting
{
    public class DependentProjectBuilder : IBuildManager
    {
        private readonly INugetTestSetup _nugetTestSetup;
        private readonly IIOUtilities _ioUtilities;
        private readonly IDotnetMsBuildProjectBuilder _projectBuilder;
        private string? _tempDependentProjectsDirectory;

        public DependentProjectBuilder(CommandPaths? commandPaths = null)
            : this(
                commandPaths,
                new NugetTestSetup(
                    MsBuildProjectHelper.Instance,
                    new NugetTempEnvironmentManager(
                        IOUtilities.Instance,
                        new NugetAddCommand(),
                        MsBuildProjectHelper.Instance,
                        new NugetGlobalPackagesPathProvider())),
                new IOUtilities(),
                new DotnetMsBuildProjectBuilder())
        {
        }

        internal DependentProjectBuilder(
            CommandPaths? commandPaths,
            INugetTestSetup nugetTestSetup,
            IIOUtilities ioUtilities,
            IDotnetMsBuildProjectBuilder projectBuilder)
        {
            _nugetTestSetup = nugetTestSetup;
            _ioUtilities = ioUtilities;
            _projectBuilder = projectBuilder;
            if (commandPaths == null)
            {
                return;
            }

            nugetTestSetup.NugetCommandPath = commandPaths.Nuget;
            projectBuilder.SetCommandPaths(commandPaths.DotNet, commandPaths.MsBuild);
        }

        private string GetUniqueDependentProjectContainingDirectory()
        {
            _tempDependentProjectsDirectory ??= _ioUtilities.CreateTempDirectory();
            return _ioUtilities.CreateUniqueSubdirectory(_tempDependentProjectsDirectory);
        }

        public IProject NewProject() => new Project(this);

        BuildResult IBuildManager.Build(ProjectBuildContext projectContext, bool isDotnet, string arguments)
        {
            System.Xml.Linq.XDocument dependentProject = _ioUtilities.XDocParse(projectContext.ProjectContents);
            _nugetTestSetup.Setup(projectContext.NuPkgPath, dependentProject);

            string projectContainingDirectory = GetUniqueDependentProjectContainingDirectory();

            string projectFilePath = Path.Combine(projectContainingDirectory, projectContext.ProjectRelativePath);
            string? projectDirectoryPath = Path.GetDirectoryName(projectFilePath);
            string projectFileName = Path.GetFileName(projectFilePath);

            _ = _ioUtilities.SaveXDocumentToDirectory(dependentProject, projectDirectoryPath!, projectFileName);

            this.AddFiles(projectContext.Files, projectContainingDirectory);

            var projectDirectory = new DirectoryInfo(projectDirectoryPath!);
            var containingDirectory = new DirectoryInfo(projectContainingDirectory);
            ProcessResult processResult = _projectBuilder.Build(projectFilePath, isDotnet, arguments, projectContainingDirectory);

            void AddFiles(IEnumerable<(string Contents, string RelativePath)> files)
                => this.AddFiles(files, projectContainingDirectory);
            ProcessResult Rebuild(string? newArgs)
            {
                arguments = newArgs ?? arguments;
                return _projectBuilder.Build(projectFilePath, isDotnet, arguments, projectContainingDirectory);
            }

            return new BuildResult(projectDirectory, containingDirectory, processResult, AddFiles, Rebuild);
        }

        private void AddFiles(IEnumerable<(string Contents, string RelativePath)>? files, string projectContainingDirectory)
        {
            if (files == null)
            {
                return;
            }

            foreach ((string Contents, string RelativePath) in files)
            {
                _ioUtilities.AddRelativeFile(projectContainingDirectory, RelativePath, Contents);
            }
        }

        public void TearDown()
        {
            _nugetTestSetup.TearDown();
            _ioUtilities.TryDeleteDirectoryRecursive(_tempDependentProjectsDirectory);
        }
    }
}
