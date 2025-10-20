using NugetBuildTargetsIntegrationTesting.Builder;
using NugetBuildTargetsIntegrationTesting.Building;
using NugetBuildTargetsIntegrationTesting.IO;
using NugetBuildTargetsIntegrationTesting.MSBuildHelpers;
using NugetBuildTargetsIntegrationTesting.Nuget;
using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting
{
    public class NugetBuildTargetsTestSetupBuilder : IBuildManager
    {
        private readonly INugetTestSetup _nugetTestSetup;
        private readonly IIOUtilities _ioUtilities;
        private readonly IDotnetMsBuildProjectBuilder projectBuilder;
        private string? _tempDependentProjectsDirectory;

        public NugetBuildTargetsTestSetupBuilder()
            : this(null)
        {
        }

        public NugetBuildTargetsTestSetupBuilder(CommandPaths? commandPaths)
            : this(
                commandPaths,
                new NugetTestSetup(
                    MsBuildProjectHelper.Instance,
                    new NugetTempEnvironmentManager(
                        IOUtilities.Instance,
                        new NugetAddCommand(),
                        MsBuildProjectHelper.Instance)
                ),
                new IOUtilities(),
                new DotnetMsBuildProjectBuilder())
        {
        }

        internal NugetBuildTargetsTestSetupBuilder(
            CommandPaths? commandPaths,
            INugetTestSetup nugetTestSetup,
            IIOUtilities ioUtilities,
            IDotnetMsBuildProjectBuilder projectBuilder
            )
        {
            _nugetTestSetup = nugetTestSetup;
            _ioUtilities = ioUtilities;
            this.projectBuilder = projectBuilder;
            if (commandPaths != null)
            {
                nugetTestSetup.NugetCommandPath = commandPaths.Nuget;
                projectBuilder.SetCommandPaths(commandPaths.DotNet, commandPaths.MsBuild);
            }
        }

        private string GetUniqueDependentProjectContainingDirectory()
        {
            _tempDependentProjectsDirectory ??= _ioUtilities.CreateTempDirectory();
            return _ioUtilities.CreateUniqueSubdirectory(_tempDependentProjectsDirectory);
        }

        public IProject CreateProject() => new Project(this);

        BuildResult IBuildManager.Build(ProjectBuildContext projectContext, bool isDotnet, string arguments)
        {
            var dependentProject = _ioUtilities.XDocParse(projectContext.ProjectContents);
            _nugetTestSetup.Setup(projectContext.NuPkgPath, dependentProject);

            string projectContainingDirectory = GetUniqueDependentProjectContainingDirectory();

            var projectFilePath = Path.Combine(projectContainingDirectory, projectContext.ProjectRelativePath);
            var projectDirectoryPath = Path.GetDirectoryName(projectFilePath);
            var projectFileName = Path.GetFileName(projectFilePath);

            _ioUtilities.SaveXDocumentToDirectory(dependentProject, projectDirectoryPath!, projectFileName);

            AddFiles(projectContext.Files, projectContainingDirectory);

            var projectDirectory = new DirectoryInfo(projectDirectoryPath!);
            var containingDirectory = new DirectoryInfo(projectContainingDirectory);
            var processResult = projectBuilder.Build(projectFilePath, isDotnet, arguments, projectContainingDirectory);

            
            Action<IEnumerable<(string Contents, string RelativePath)>> addFiles = (files) =>
            {
                AddFiles(files, projectContainingDirectory);
            };
            Func<string?, ProcessResult> rebuild = newArgs =>
            {
                arguments = newArgs ?? arguments;
                return projectBuilder.Build(projectFilePath, isDotnet, arguments, projectContainingDirectory);
            };
            return new BuildResult(projectDirectory, containingDirectory, processResult,addFiles, rebuild);
        }

        private void AddFiles(IEnumerable<(string Contents, string RelativePath)>? files, string projectContainingDirectory)
        {
            if (files != null)
            {
                foreach (var (Contents, RelativePath) in files)
                {
                    _ioUtilities.AddRelativeFile(projectContainingDirectory, RelativePath, Contents);
                }
            }
        }

        public void TearDown()
        {
            _nugetTestSetup.TearDown();
            _ioUtilities.TryDeleteDirectoryRecursive(_tempDependentProjectsDirectory);
        }
    }
}
