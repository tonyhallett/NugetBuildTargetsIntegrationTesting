using System.Xml.Linq;
using Moq;
using NugetBuildTargetsIntegrationTesting;
using NugetBuildTargetsIntegrationTesting.Builder;
using NugetBuildTargetsIntegrationTesting.Building;
using NugetBuildTargetsIntegrationTesting.IO;
using NugetBuildTargetsIntegrationTesting.Nuget;
using NugetBuildTargetsIntegrationTesting.Processing;

namespace UnitTests
{
    internal sealed class DependentProjectBuilder_Tests
    {
        private const string ProjectContainingDir = "projectcontainingdir";
        private const string NuPkgPath = "package.nupkg";
        private const string ProjectContent = "<Project></Project>";
        private const string BuildArgs = "somebuildargs";
        private readonly CommandPaths _commandPaths = new("nugetPath", "msbuildPath", "dotnetPath");
        private Mock<INugetTestSetup> _mockNugetTestSetup = new();
        private Mock<IIOUtilities> _mockIOUtiities = new();
        private Mock<IDotnetMsBuildProjectBuilder> _mockDotNetMsBuildProjectBuilder = new();
        private DependentProjectBuilder? _nugetBuildTargetsTestSetupBuilder;
        private XDocument _projectXDoc = new();

        [SetUp]
        public void Setup()
        {
            _projectXDoc = XDocument.Parse(ProjectContent);
            _mockNugetTestSetup = new Mock<INugetTestSetup>();
            _ = _mockNugetTestSetup.Setup(mock => mock.Setup(NuPkgPath, _projectXDoc))
                .Callback<string, XDocument>((_, project) => NugetTestSetupModifyProject(project));
            _mockIOUtiities = new Mock<IIOUtilities>();
            _ = _mockIOUtiities.Setup(mock => mock.CreateTempDirectory()).Returns("rootdir");
            _ = _mockIOUtiities.Setup(mock => mock.CreateUniqueSubdirectory("rootdir")).Returns(ProjectContainingDir);
            _ = _mockIOUtiities.Setup(mock => mock.XDocParse(ProjectContent)).Returns(_projectXDoc);
            _mockDotNetMsBuildProjectBuilder = new Mock<IDotnetMsBuildProjectBuilder>();
            _nugetBuildTargetsTestSetupBuilder = new DependentProjectBuilder(
               _commandPaths,
               _mockNugetTestSetup.Object,
               _mockIOUtiities.Object,
               _mockDotNetMsBuildProjectBuilder.Object);

            static void NugetTestSetupModifyProject(XDocument project) => project.Root!.Add(new XElement("NugetSetupDone"));
        }

        [Test]
        public void Should_Set_Command_Paths_If_Provided()
        {
            _mockNugetTestSetup.VerifySet(mockNugetTestSetup => mockNugetTestSetup.NugetCommandPath = _commandPaths.Nuget, Times.Once);
            _mockDotNetMsBuildProjectBuilder.Verify(
                mockDotNetMsBuildProjectBuilder =>
                    mockDotNetMsBuildProjectBuilder.SetCommandPaths(_commandPaths.DotNet, _commandPaths.MsBuild),
                Times.Once);
        }

        private IBuildResult Build(
            string? projectRelativePath = null,
            List<(string Contents, string RelativePath)>? files = null,
            bool dotNetBuild = true,
            string args = BuildArgs)
        {
            IProject project = _nugetBuildTargetsTestSetupBuilder!.NewProject();
            if (files != null)
            {
                project = project.AddFiles(files);
            }

            IAddNuget addNuget = projectRelativePath == null ? project.AddProject(ProjectContent) : project.AddProject(ProjectContent, projectRelativePath);
            IProjectBuilder projectBuilder = addNuget.AddNuPkg(NuPkgPath);
            return dotNetBuild ? projectBuilder.BuildWithDotNet(args) : projectBuilder.BuildWithMSBuild(args);
        }

        [Test]
        public void Should_NugetTestSetup_Setup_On_Build()
        {
            _ = Build();

            _mockNugetTestSetup.Verify(
                mockNugetTestSetup =>
                    mockNugetTestSetup.Setup(NuPkgPath, _projectXDoc),
                Times.Once);
        }

        [TestCase("dep.csproj", ProjectContainingDir)]
        [TestCase("subdir/dep.csproj", $"{ProjectContainingDir}\\subdir")]
        public void Should_Save_The_Project_To_Unique_Directory_Relatively_On_Build(
            string projectRelativePath,
            string expectedProjectDirectory)
        {
            bool didSave = false;
            _ = _mockIOUtiities.Setup(mock => mock.SaveXDocumentToDirectory(_projectXDoc, expectedProjectDirectory, "dep.csproj"))
                .Callback<XDocument, string, string>((xdoc, _, __) =>
                {
                    // assert that the XDocument has been modified by NugetTestSetup
                    Assert.That(xdoc.Root!.Element("NugetSetupDone"), Is.Not.Null);
                    didSave = true;
                });

            _ = Build(projectRelativePath);

            Assert.That(didSave, Is.True);

        }

        [Test]
        public void Should_Add_All_Files_Relative_To_The_Project_Containing_Directory()
        {
            List<(string Contents, string RelativePath)> files =
            [
                ("contents", "subdir/rel.txt")
            ];
            _ = Build(null, files);

            _mockIOUtiities.Verify(
                mock => mock.AddRelativeFile(ProjectContainingDir, "subdir/rel.txt", "contents"),
                Times.Once);
        }

        [TestCase(true, 0, true)]
        [TestCase(false, 9, false)]
        public void Should_Build_Using_DotnetMsBuildProjectBuilder(bool dotNetBuild, int exitCode, bool expectedPassed)
        {
            var processResult = new ProcessResult("output", "error", exitCode);
            _ = _mockDotNetMsBuildProjectBuilder.Setup(
                mockDotNetMsBuildProjectBuilder =>
                    mockDotNetMsBuildProjectBuilder.Build(
                        $"{ProjectContainingDir}\\{Project.DefaultProjectRelativePath}",
                        dotNetBuild,
                        BuildArgs,
                        ProjectContainingDir)).Returns(processResult);

            IBuildResult buildResult = Build(null, null, dotNetBuild);

            Assert.Multiple(() =>
            {
                Assert.That(buildResult.Passed, Is.EqualTo(expectedPassed));
                Assert.That(buildResult.StandardError, Is.EqualTo("error"));
                Assert.That(buildResult.StandardOutput, Is.EqualTo("output"));
            });
        }

        [Test]
        public void Should_Be_Able_To_AddFiles_After_First_Build()
        {
            var processResult = new ProcessResult(string.Empty, string.Empty, 0);
            _ = _mockDotNetMsBuildProjectBuilder.Setup(
                mockDotNetMsBuildProjectBuilder =>
                    mockDotNetMsBuildProjectBuilder.Build(
                        $"{ProjectContainingDir}\\{Project.DefaultProjectRelativePath}",
                        true,
                        BuildArgs,
                        ProjectContainingDir)).Returns(processResult);

            IBuildResult buildResult = Build(null, null, true);

            const string rebuildFileContents = "contents";
            const string rebuildRelativeFilePath = "relative.txt";
            _ = buildResult.AddFiles([(rebuildFileContents, rebuildRelativeFilePath)]);

            _mockIOUtiities.Verify(ioUtilities => ioUtilities.AddRelativeFile(ProjectContainingDir, rebuildRelativeFilePath, rebuildFileContents));
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void Should_Rebuild_With_Same_Command_And_Args_By_Default(bool dotnetBuild, bool passed)
        {
            const string customArgs = "custom args";
            var processResult1 = new ProcessResult("output1", "error1", 0);
            var processResult2 = new ProcessResult("output2", "error2", passed ? 0 : 1);
            _ = _mockDotNetMsBuildProjectBuilder.SetupSequence(
                mockDotNetMsBuildProjectBuilder =>
                    mockDotNetMsBuildProjectBuilder.Build(
                        $"{ProjectContainingDir}\\{Project.DefaultProjectRelativePath}",
                        dotnetBuild,
                        customArgs,
                        ProjectContainingDir)).Returns(processResult1).Returns(processResult2);

            IBuildResult buildResult = Build(null, null, dotnetBuild, customArgs);

            buildResult.Rebuild();

            Assert.Multiple(() =>
            {
                Assert.That(buildResult.StandardOutput, Is.EqualTo("output2"));
                Assert.That(buildResult.StandardError, Is.EqualTo("error2"));
                Assert.That(buildResult.ErrorAndOutput, Is.EqualTo("error2" + Environment.NewLine + "output2"));
                Assert.That(buildResult.Passed, Is.EqualTo(passed));
            });
        }

        [Test]
        public void Should_Create_All_Project_Containing_Directories_In_Same_Temp_Directory()
        {
            _ = Build();
            _ = Build();

            _mockIOUtiities.Verify(mock => mock.CreateTempDirectory(), Times.Once);
        }

        [Test]
        public void Should_TearDown()
        {
            _ = Build();

            _nugetBuildTargetsTestSetupBuilder!.TearDown();

            _mockNugetTestSetup.Verify(mock => mock.TearDown(), Times.Once);
            _mockIOUtiities.Verify(mock => mock.TryDeleteDirectoryRecursive("rootdir"), Times.Once);
        }
    }
}
