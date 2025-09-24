using System.Xml.Linq;
using Moq;
using NugetBuildTargetsIntegrationTesting;

namespace UnitTests
{
    public class NugetBuildTargetsTestSetup_Tests
    {
        private Mock<INugetTestSetup> _mockNugetTestSetup;
        private Mock<IProjectBuilder> _mockProjectBuilder;
        private Mock<IIOUtilities> _mockIOUtilities;
        private NugetBuildTargetsTestSetup _nugetBuildTargetsTestSetup;
        private const string projectStr = ".proj as string";
        private const string nupkgPath = "nupkg path";
        private const string projectBuilderReturn = "from project builder";
        private XDocument _project;
        private bool projectBuilderBuilt;

        [SetUp]
        public void Setup()
        {
            _mockNugetTestSetup = new Mock<INugetTestSetup>();
            _mockProjectBuilder = new Mock<IProjectBuilder>();
            _mockProjectBuilder.Setup(projectBuilder => projectBuilder.Build("projectpath"))
                .Returns(projectBuilderReturn)
                .Callback(() => projectBuilderBuilt = true);
            _mockIOUtilities = new Mock<IIOUtilities>();
            _project = new XDocument();
            _mockIOUtilities.Setup(ioUtilities => ioUtilities.XDocParse(projectStr)).Returns(_project);
            _mockIOUtilities.Setup(ioUtilities => ioUtilities.CreateTempDirectory()).Returns("temp dir");
            _mockIOUtilities.SetupSequence(ioUtilities => ioUtilities.CreateUniqueSubdirectory("temp dir"))
                .Returns("unique sub dir 1").Returns("unique sub dir 2");
            _nugetBuildTargetsTestSetup = new NugetBuildTargetsTestSetup(
                _mockNugetTestSetup.Object,
                _mockProjectBuilder.Object,
                _mockIOUtilities.Object);
        }

        [Test]
        public void Should_Not_Throw_If_Callback_Is_Null()
        {
            _ = InvokeSetup();
        }

        [Test]
        public void Should_NugetTestSetup_With_Parsed_XDocument()
        {
            _ = InvokeSetup();
            _mockNugetTestSetup.Verify(nugetTestSetup => nugetTestSetup.Setup(nupkgPath, _project), Times.Once);
        }


        [Test]
        public void Should_Save_Project_To_Unique_Sub_Directory_Of_Temp_Directory()
        {
            _ = _nugetBuildTargetsTestSetup.Setup(projectStr, nupkgPath);
            _ = _nugetBuildTargetsTestSetup.Setup(projectStr, nupkgPath);

            _mockIOUtilities.Verify(ioUtilities => ioUtilities.SaveXDocumentToDirectory(_project, "unique sub dir 1", "DependentProject.csproj"));
            _mockIOUtilities.Verify(ioUtilities => ioUtilities.SaveXDocumentToDirectory(_project, "unique sub dir 2", "DependentProject.csproj"));
        }

        [Test]
        public void Should_Invoke_Callback_With_Project_Path_Then_Build()
        {
            _mockIOUtilities.Setup(ioUtilities => ioUtilities.SaveXDocumentToDirectory(_project, "unique sub dir 1", "DependentProject.csproj")).Returns("projectpath");
            var invokedCallback = true;
            var buildResult = InvokeSetup(projectPath =>
            {
                Assert.Multiple(() =>
                {
                    Assert.That(projectPath, Is.EqualTo("projectpath"));
                    Assert.That(projectBuilderBuilt, Is.False);
                });
                invokedCallback = true;
            });

            Assert.Multiple(() =>
            {
                Assert.That(invokedCallback, Is.True);
                Assert.That(buildResult, Is.EqualTo(projectBuilderReturn));
            });

        }

        [Test]
        public void Should_CleanUp_In_TearDown()
        {
            InvokeSetup();
            _nugetBuildTargetsTestSetup.TearDown();

            _mockNugetTestSetup.Verify(nugetTestSetup => nugetTestSetup.TearDown());

            _mockIOUtilities.Verify(ioUtilities => ioUtilities.TryDeleteDirectoryRecursive("temp dir"));
        }

        private string InvokeSetup(Action<string>? callback = null)
            => _nugetBuildTargetsTestSetup.Setup(projectStr, nupkgPath, callback);
    }
}