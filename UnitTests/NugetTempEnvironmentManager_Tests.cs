using System.Xml.Linq;
using Moq;
using NugetBuildTargetsIntegrationTesting.IO;
using NugetBuildTargetsIntegrationTesting.MSBuildHelpers;
using NugetBuildTargetsIntegrationTesting.Nuget;
using NugetBuildTargetsIntegrationTesting.Processing;

namespace UnitTests
{
    internal sealed class NugetTempEnvironmentManager_Tests
    {
        private readonly XDocument _project = new();
        private Mock<IIOUtilities> _mockIOUtilities = new();
        private Mock<INugetAddCommand> _mockNugetAddCommand = new();
        private Mock<IMsBuildProjectHelper> _mockMsBuildProjectHelper = new();
        private NugetTempEnvironmentManager? _nugetTempEnvironmentManager;

        [SetUp]
        public void SetUp()
        {
            _mockIOUtilities = new Mock<IIOUtilities>();
            _ = _mockIOUtilities.Setup(ioUtilities => ioUtilities.CreateTempDirectory()).Returns("tempdir");
            _mockNugetAddCommand = new Mock<INugetAddCommand>();
            _mockMsBuildProjectHelper = new Mock<IMsBuildProjectHelper>();
            var mockNuGetGlobalPackagesPathProvider = new Mock<INuGetGlobalPackagesPathProvider>();
            _ = mockNuGetGlobalPackagesPathProvider.Setup(nugetGlobalPackagesPathProvider => nugetGlobalPackagesPathProvider.Provide())
                .Returns("global");
            _nugetTempEnvironmentManager = new NugetTempEnvironmentManager(
                _mockIOUtilities.Object,
                _mockNugetAddCommand.Object,
                _mockMsBuildProjectHelper.Object,
                mockNuGetGlobalPackagesPathProvider.Object);
        }

        [Test]
        public async Task Should_Add_The_Package_To_Temp_Nuget_Source()
        {
            _ = _mockNugetAddCommand.Setup(nugetAddCommand => nugetAddCommand.AddPackageToSourceAsync("nupkgPath", "tempdir", "addcommandpath"))
                .ReturnsAsync(new ProcessResult(string.Empty, string.Empty, 0));

            await _nugetTempEnvironmentManager!.SetupAsync("nupkgPath", _project, "packageInstallPath", "addcommandpath");

            _mockNugetAddCommand.VerifyAll();
        }

        [Test]
        public async Task Should_Add_The_Package_To_Temp_Nuget_Source_Only_Once()
        {
            _ = _mockNugetAddCommand.Setup(nugetAddCommand => nugetAddCommand.AddPackageToSourceAsync("nupkgPath", "tempdir", null))
                .ReturnsAsync(new ProcessResult(string.Empty, string.Empty, 0));

            await _nugetTempEnvironmentManager!.SetupAsync("nupkgPath", _project, "packageInstallPath", null);
            await _nugetTempEnvironmentManager!.SetupAsync("nupkgPath", _project, "packageInstallPath", null);

            _mockIOUtilities.Verify(ioUtilities => ioUtilities.CreateTempDirectory(), Times.Once);
            _mockNugetAddCommand.Verify(nugetAddCommand => nugetAddCommand.AddPackageToSourceAsync("nupkgPath", "tempdir", null), Times.Once);
        }

        [Test]
        public async Task Should_Insert_MSBuild_Properties_RestoreSources_RestorePackagesPath_In_New_PropertyGroup()
        {
            _ = _mockNugetAddCommand.Setup(nugetAddCommand => nugetAddCommand.AddPackageToSourceAsync("nupkgPath", "tempdir", null))
                .ReturnsAsync(new ProcessResult(string.Empty, string.Empty, 0));

            var propertyGroup = new XElement("PropertyGroup");
            _ = _mockMsBuildProjectHelper.Setup(msBuildProjectHelper => msBuildProjectHelper.InsertPropertyGroup(_project))
                .Returns(propertyGroup);

            await _nugetTempEnvironmentManager!.SetupAsync("nupkgPath", _project, "packageInstallPath", null);

            _mockMsBuildProjectHelper.Verify(msBuildProjectHelper => msBuildProjectHelper.AddProperty(propertyGroup, "RestoreSources", "tempdir;global;https://api.nuget.org/v3/index.json"));
            _mockMsBuildProjectHelper.Verify(msBuildProjectHelper => msBuildProjectHelper.AddProperty(propertyGroup, "RestorePackagesPath", "packageInstallPath"));
        }

        [Test]
        public void Should_Throw_NugetAddException_When_NugetAddCommand_ExitCode_Not_0()
        {
            var failingProcessResult = new ProcessResult("output", "error", 1);
            _ = _mockNugetAddCommand.Setup(nugetAddCommand => nugetAddCommand.AddPackageToSourceAsync("nupkgPath", "tempdir", null))
                .ReturnsAsync(failingProcessResult);

            NugetAddException? nugetAddException = Assert.ThrowsAsync<NugetAddException>(() => _nugetTempEnvironmentManager!.SetupAsync("nupkgPath", _project, "packageInstallPath", null));
            Assert.Multiple(() =>
            {
                Assert.That(nugetAddException!.StandardError, Is.EqualTo(failingProcessResult.StandardError));
                Assert.That(nugetAddException!.StandardOutput, Is.EqualTo(failingProcessResult.StandardOutput));
                Assert.That(nugetAddException!.ExitCode, Is.EqualTo(failingProcessResult.ExitCode));
            });
        }

        [Test]
        public async Task Should_TryDeleteDirectoryRecursively_The_Temp_Local_Feed_Directory_On_CleanUp()
        {
            _ = _mockNugetAddCommand.Setup(nugetAddCommand => nugetAddCommand.AddPackageToSourceAsync("nupkgPath", "tempdir", null))
                .ReturnsAsync(new ProcessResult(string.Empty, string.Empty, 0));
            await _nugetTempEnvironmentManager!.SetupAsync("nupkgPath", _project, "packageInstallPath", null);

            _nugetTempEnvironmentManager.CleanUp();

            _mockIOUtilities.Verify(ioUtilities => ioUtilities.TryDeleteDirectoryRecursive("tempdir"));
        }
    }
}
