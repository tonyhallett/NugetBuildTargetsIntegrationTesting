using System.Xml.Linq;
using Moq;
using NugetBuildTargetsIntegrationTesting;

namespace UnitTests
{
    public class NugetTestSetup_Tests
    {
        [Test]
        public void Should_Setup_The_Nuget_Temp_Environment_To_Install_Packages_Withing_Project_Output()
        {
            var mockNugetTempEnvironmentManager = new Mock<INugetTempEnvironmentManager>();
            var nugetTestSetup = new NugetTestSetup(new Mock<IMsBuildProjectHelper>().Object, mockNugetTempEnvironmentManager.Object);
            var project = new XDocument();
            nugetTestSetup.Setup("path.nupkg", project);

            mockNugetTempEnvironmentManager.Verify(nugetTempEnvironmentManager => nugetTempEnvironmentManager.Setup("path.nupkg", project, "$(BaseIntermediateOutputPath)\\packages"));
        }

        [Test]
        public void Should_AddPackageReference_To_The_Nupkg_In_The_Dependent_Project()
        {
            var mockMsBuildProjectHelper = new Mock<IMsBuildProjectHelper>();
            var nugetTestSetup = new NugetTestSetup(mockMsBuildProjectHelper.Object, new Mock<INugetTempEnvironmentManager>().Object);
            var project = new XDocument();
            nugetTestSetup.Setup("path.nupkg", project);

            mockMsBuildProjectHelper.Verify(msBuildProjectHelper => msBuildProjectHelper.AddPackageReference(project, "path.nupkg"));
        }

        [Test]
        public void Should_CleanUp_NugetTempEnvironmentManager_When_TearDown()
        {
            var mockNugetTempEnvironmentManager = new Mock<INugetTempEnvironmentManager>();
            var nugetTestSetup = new NugetTestSetup(new Mock<IMsBuildProjectHelper>().Object, mockNugetTempEnvironmentManager.Object);

            nugetTestSetup.TearDown();

            mockNugetTempEnvironmentManager.Verify(nugetTempEnvironmentManager => nugetTempEnvironmentManager.CleanUp());
        }
    }
}