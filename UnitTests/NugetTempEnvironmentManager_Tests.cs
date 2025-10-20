using System.Xml.Linq;
using Moq;
using NugetBuildTargetsIntegrationTesting.IO;
using NugetBuildTargetsIntegrationTesting.MSBuildHelpers;
using NugetBuildTargetsIntegrationTesting.Nuget;
using NugetBuildTargetsIntegrationTesting.Processing;

namespace UnitTests
{
    public class NugetTempEnvironmentManager_Tests
    {
        private Mock<IIOUtilities> _mockIOUtilities;
        private Mock<INugetAddCommand> _mockNugetAddCommand;
        private Mock<IMsBuildProjectHelper> _mockMsBuildProjectHelper;
        private NugetTempEnvironmentManager _nugetTempEnvironmentManager;
        private readonly XDocument project = new XDocument();

        [SetUp]
        public void SetUp()
        {
            _mockIOUtilities = new Mock<IIOUtilities>();
            _mockIOUtilities.Setup(ioUtilities => ioUtilities.CreateTempDirectory()).Returns("tempdir");
            _mockNugetAddCommand = new Mock<INugetAddCommand>();
            _mockMsBuildProjectHelper = new Mock<IMsBuildProjectHelper>();
            _nugetTempEnvironmentManager = new NugetTempEnvironmentManager(_mockIOUtilities.Object, _mockNugetAddCommand.Object, _mockMsBuildProjectHelper.Object);
        }


        [Test]
        public void Should_Add_The_Package_To_Temp_Nuget_Source()
        {
            _mockNugetAddCommand.Setup(nugetAddCommand => nugetAddCommand.AddPackageToSource("nupkgPath", "tempdir", "addcommandpath"))
                .Returns(new ProcessResult("", "", 0));

            _nugetTempEnvironmentManager.Setup("nupkgPath", project, "packageInstallPath", "addcommandpath");

            _mockNugetAddCommand.VerifyAll();
        }

        [Test]
        public void Should_Add_The_Package_To_Temp_Nuget_Source_Only_Once()
        {
            _mockNugetAddCommand.Setup(nugetAddCommand => nugetAddCommand.AddPackageToSource("nupkgPath", "tempdir", null))
                .Returns(new ProcessResult("", "", 0));

            _nugetTempEnvironmentManager.Setup("nupkgPath", project, "packageInstallPath", null);
            _nugetTempEnvironmentManager.Setup("nupkgPath", project, "packageInstallPath", null);

            _mockIOUtilities.Verify(ioUtilities => ioUtilities.CreateTempDirectory(), Times.Once);
            _mockNugetAddCommand.Verify(nugetAddCommand => nugetAddCommand.AddPackageToSource("nupkgPath", "tempdir", null), Times.Once);
        }

        [Test]
        public void Should_Insert_MSBuild_Properties_RestoreSources_RestorePackagesPath_Ine_New_PropertyGroup()
        {
            _mockNugetAddCommand.Setup(nugetAddCommand => nugetAddCommand.AddPackageToSource("nupkgPath", "tempdir", null))
                .Returns(new ProcessResult("", "", 0));

            var propertyGroup = new XElement("PropertyGroup");
            _mockMsBuildProjectHelper.Setup(msBuildProjectHelper => msBuildProjectHelper.InsertPropertyGroup(project)).Returns(propertyGroup);

            _nugetTempEnvironmentManager.Setup("nupkgPath", project, "packageInstallPath", null);
            
            _mockMsBuildProjectHelper.Verify(msBuildProjectHelper => msBuildProjectHelper.AddProperty(propertyGroup, "RestoreSources", "tempdir;"));
            _mockMsBuildProjectHelper.Verify(msBuildProjectHelper => msBuildProjectHelper.AddProperty(propertyGroup, "RestorePackagesPath", "packageInstallPath"));
        }

        [Test]
        public void Should_Throw_NugetAddException_When_NugetAddCommand_ExitCode_Not_0()
        {
            _mockNugetAddCommand.Setup(nugetAddCommand => nugetAddCommand.AddPackageToSource("nupkgPath", "tempdir", null))
                .Returns(new ProcessResult("output", "error", 1));

            Assert.That(() => _nugetTempEnvironmentManager.Setup("nupkgPath", project, "packageInstallPath", null), Throws.InstanceOf<NugetAddException>());
        }

        [Test]
        public void Should_TryDeleteDirectoryRecursively_The_Temp_Local_Feed_Directory_On_CleanUp()
        {
            _mockNugetAddCommand.Setup(nugetAddCommand => nugetAddCommand.AddPackageToSource("nupkgPath", "tempdir", null))
                .Returns(new ProcessResult("", "", 0));
            _nugetTempEnvironmentManager.Setup("nupkgPath", project, "packageInstallPath", null);

            _nugetTempEnvironmentManager.CleanUp();

            _mockIOUtilities.Verify(ioUtilities => ioUtilities.TryDeleteDirectoryRecursive("tempdir"));
        }
    }
}