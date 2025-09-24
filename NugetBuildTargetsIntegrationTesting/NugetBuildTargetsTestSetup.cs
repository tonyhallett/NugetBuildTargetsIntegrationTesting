using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting
{
    public class NugetBuildTargetsTestSetup
    {
        private readonly INugetTestSetup _nugetTestSetup;
        private readonly IProjectBuilder _projectBuilder;
        private readonly IIOUtilities _ioUtilities;
        private string? _tempDependentProjectsDirectory;

        public NugetBuildTargetsTestSetup() : 
            this(new NugetTestSetup(
                MsBuildProjectHelper.Instance,
                new NugetTempEnvironmentManager(
                    IOUtilities.Instance, 
                    new NugetAddCommand(), 
                    MsBuildProjectHelper.Instance)), 
                new DotNetProjectBuilder(), 
                IOUtilities.Instance)
        {
        }

        public NugetBuildTargetsTestSetup(IProjectBuilder projectBuilder) : 
            this(new NugetTestSetup(
                MsBuildProjectHelper.Instance,
                new NugetTempEnvironmentManager(
                    IOUtilities.Instance,
                    new NugetAddCommand(),
                    MsBuildProjectHelper.Instance)),
                projectBuilder,
                IOUtilities.Instance)
        {
        }

        public NugetBuildTargetsTestSetup(INugetAddCommand nugetAddCommand) :
            this(new NugetTestSetup(
                MsBuildProjectHelper.Instance,
                new NugetTempEnvironmentManager(
                    IOUtilities.Instance,
                    nugetAddCommand,
                    MsBuildProjectHelper.Instance)),
                new DotNetProjectBuilder(),
                IOUtilities.Instance)
        {
        }

        public NugetBuildTargetsTestSetup(IProjectBuilder projectBuilder,INugetAddCommand nugetAddCommand) :
            this(new NugetTestSetup(
                MsBuildProjectHelper.Instance,
                new NugetTempEnvironmentManager(
                    IOUtilities.Instance,
                    nugetAddCommand,
                    MsBuildProjectHelper.Instance)),
                projectBuilder,
                IOUtilities.Instance)
        {
        }

        internal NugetBuildTargetsTestSetup(
            INugetTestSetup nugetTestSetup,
            IProjectBuilder projectBuilder,
            IIOUtilities ioUtilities
            )
        {
            _nugetTestSetup = nugetTestSetup;
            _projectBuilder = projectBuilder;
            _ioUtilities = ioUtilities;
        }

        public string Setup(string dependentProjectContents, string nupkgPath, Action<string>? projectPathCallback = null)
        {
            var dependentProject = _ioUtilities.XDocParse(dependentProjectContents);
            _nugetTestSetup.Setup(nupkgPath, dependentProject);
            var projectPath = SaveDependentProjectToTempDirectory(dependentProject);

            projectPathCallback?.Invoke(projectPath);

            return _projectBuilder.Build(projectPath);
        }

        private string SaveDependentProjectToTempDirectory(XDocument dependentProject)
            => _ioUtilities.SaveXDocumentToDirectory(dependentProject, GetUniqueDependentProjectDirectory(), "DependentProject.csproj");

        private string GetUniqueDependentProjectDirectory()
        {
            _tempDependentProjectsDirectory ??= _ioUtilities.CreateTempDirectory();
            return _ioUtilities.CreateUniqueSubdirectory(_tempDependentProjectsDirectory);
        }

        public void TearDown()
        {
            _nugetTestSetup.TearDown();
            _ioUtilities.TryDeleteDirectoryRecursive(_tempDependentProjectsDirectory);
        }
    }
}