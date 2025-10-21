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
    internal class DependentProjectBuilder_Tests
    {
        private Mock<INugetTestSetup> _mockNugetTestSetup;
        private Mock<IIOUtilities> _mockIOUtiities;
        private Mock<IDotnetMsBuildProjectBuilder> _mockDotNetMsBuildProjectBuilder;
        private DependentProjectBuilder _nugetBuildTargetsTestSetupBuilder;
        private const string projectContainingDir = "projectcontainingdir";
        private const string nuPkgPath = "package.nupkg";
        private const string projectContent = "<Project></Project>";
        private XDocument _projectXDoc = new XDocument();
        private readonly CommandPaths _commandPaths = new CommandPaths("nugetPath", "msbuildPath", "dotnetPath");
        private const string _buildArgs = "somebuildargs";

        [SetUp]
        public void Setup()
        {
            _projectXDoc = XDocument.Parse(projectContent);
            _mockNugetTestSetup = new Mock<INugetTestSetup>();
            _mockNugetTestSetup.Setup(mock => mock.Setup(nuPkgPath, _projectXDoc)).Callback<string, XDocument>((path, xdoc) =>
            {
                // Simulate modifying the XDocument during setup
                xdoc.Root!.Add(new XElement("NugetSetupDone"));
            });
            _mockIOUtiities = new Mock<IIOUtilities>();
            _mockIOUtiities.Setup(mock => mock.CreateTempDirectory()).Returns("rootdir");
            _mockIOUtiities.Setup(mock => mock.CreateUniqueSubdirectory("rootdir")).Returns(projectContainingDir);
            _mockIOUtiities.Setup(mock => mock.XDocParse(projectContent)).Returns(_projectXDoc);
            _mockDotNetMsBuildProjectBuilder = new Mock<IDotnetMsBuildProjectBuilder>();
            _nugetBuildTargetsTestSetupBuilder = new DependentProjectBuilder(
               _commandPaths,
               _mockNugetTestSetup.Object,
               _mockIOUtiities.Object,
               _mockDotNetMsBuildProjectBuilder.Object
               );
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
            bool dotNetBuild = true
            )
        {
            var project = _nugetBuildTargetsTestSetupBuilder.NewProject();
            if (files != null)
            {
                project = project.AddFiles(files);
            }

            IAddNuget addNuget;
            if (projectRelativePath == null)
            {
                addNuget = project.AddProject(projectContent);
            }
            else
            {
                addNuget = project.AddProject(projectContent, projectRelativePath);
            }

            IProjectBuilder projectBuilder = addNuget.AddNuPkg(nuPkgPath);
            if (dotNetBuild)
            {
                return projectBuilder.BuildWithDotNet(_buildArgs);
            }
            else
            {
                return projectBuilder.BuildWithMSBuild(_buildArgs);
            }
        }

        [Test]
        public void Should_NugetTestSetup_Setup_On_Build()
        {
            Build();

            _mockNugetTestSetup.Verify(
                mockNugetTestSetup =>
                    mockNugetTestSetup.Setup(nuPkgPath, _projectXDoc),
                Times.Once);
        }

        [TestCase("dep.csproj", projectContainingDir)]
        [TestCase("subdir/dep.csproj", $"{projectContainingDir}\\subdir")]
        public void Should_Save_The_Project_To_Unique_Directory_Relatively_On_Build(
            string projectRelativePath,
            string expectedProjectDirectory)
        {
            var didSave = false;
            _mockIOUtiities.Setup(mock => mock.SaveXDocumentToDirectory(_projectXDoc, expectedProjectDirectory, "dep.csproj"))
                .Callback<XDocument, string, string>((xdoc, dir, file) =>
                {
                    // assert that the XDocument has been modified by NugetTestSetup
                    Assert.That(xdoc.Root!.Element("NugetSetupDone"), Is.Not.Null);
                    didSave = true;
                });

            Build(projectRelativePath);

            Assert.That(didSave, Is.True);

        }

        [Test]
        public void Should_Add_All_Files_Relative_To_The_Project_Containing_Directory()
        {
            List<(string Contents, string RelativePath)> files =
            [
                ("contents","subdir/rel.txt")
            ];
            Build(null, files);

            _mockIOUtiities.Verify(mock => mock.AddRelativeFile(projectContainingDir, "subdir/rel.txt", "contents"),
                Times.Once);
        }

        [TestCase(true, 0, true)]
        [TestCase(false, 9, false)]
        public void Should_Build_Using_DotnetMsBuildProjectBuilder(bool dotNetBuild, int exitCode, bool expectedPassed)
        {
            var processResult = new ProcessResult("output", "error", exitCode);
            _mockDotNetMsBuildProjectBuilder.Setup(
                mockDotNetMsBuildProjectBuilder =>
                    mockDotNetMsBuildProjectBuilder.Build(
                        $"{projectContainingDir}\\{Project.DefaultProjectRelativePath}",
                        dotNetBuild,
                        _buildArgs,
                        projectContainingDir)).Returns(processResult);

            var buildResult = Build(null, null, dotNetBuild);

            Assert.Multiple(() =>
            {
                Assert.That(buildResult.Passed, Is.EqualTo(expectedPassed));
                Assert.That(buildResult.Error, Is.EqualTo("error"));
                Assert.That(buildResult.Output, Is.EqualTo("output"));
            });
        }

        [Test]
        public void Should_Create_All_Project_Containing_Directories_In_Same_Temp_Directory()
        {
            Build();
            Build();

            _mockIOUtiities.Verify(mock => mock.CreateTempDirectory(), Times.Once);
        }

        [Test]
        public void Should_TearDown()
        {
            Build();

            _nugetBuildTargetsTestSetupBuilder.TearDown();

            _mockNugetTestSetup.Verify(mock => mock.TearDown(), Times.Once);
            _mockIOUtiities.Verify(mock => mock.TryDeleteDirectoryRecursive("rootdir"), Times.Once);
        }
    }
}
