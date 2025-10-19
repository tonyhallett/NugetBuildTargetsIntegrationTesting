
namespace NugetBuildTargetsIntegrationTesting
{
    public class NugetBuildTargetsTestSetupBuilder : IBuildManager
    {
        private readonly INugetTestSetup _nugetTestSetup;
        private readonly IIOUtilities _ioUtilities;
        private readonly IDotnetMsBuildProjectBuilder projectBuilder;
        private string? _tempDependentProjectsDirectory;
        
        public NugetBuildTargetsTestSetupBuilder()
            :this(null)
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
        
        private string GetUniqueDependentProjectDirectory()
        {
            _tempDependentProjectsDirectory ??= _ioUtilities.CreateTempDirectory();
            return _ioUtilities.CreateUniqueSubdirectory(_tempDependentProjectsDirectory);
        }

        public IProject CreateProject() => new Project(this);

        public IBuildResult Build(ProjectBuildContext projectContext, bool isDotnet, string arguments)
        {
            var dependentProject = _ioUtilities.XDocParse(projectContext.ProjectContents);
            _nugetTestSetup.Setup(projectContext.NuPkgPath, dependentProject);

            string projectContainingDirectory = GetUniqueDependentProjectDirectory();

            var projectFilePath = Path.Combine(projectContainingDirectory, projectContext.ProjectRelativePath);
            var projectDirectoryPath = Path.GetDirectoryName(projectFilePath);
            var projectFileName = Path.GetFileName(projectFilePath);
            
            _ioUtilities.SaveXDocumentToDirectory(dependentProject, projectDirectoryPath!, projectFileName);
            
            if (projectContext.Files != null)
            {
                foreach(var (Contents, RelativePath) in projectContext.Files)
                {
                    _ioUtilities.AddRelativeFile(projectContainingDirectory, RelativePath,Contents);
                }
            }

            var projectDirectory = new DirectoryInfo(projectDirectoryPath!);
            var containingDirectory = new DirectoryInfo(projectContainingDirectory);
            var processResult = projectBuilder.Build(projectFilePath, isDotnet, arguments, projectContainingDirectory);
            return new BuildResult(projectDirectory, containingDirectory, processResult);
        }

        public void TearDown()
        {
            _nugetTestSetup.TearDown();
            _ioUtilities.TryDeleteDirectoryRecursive(_tempDependentProjectsDirectory);
        }
    }
}
