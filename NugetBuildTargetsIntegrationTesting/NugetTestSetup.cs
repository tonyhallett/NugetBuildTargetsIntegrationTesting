using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting
{
    internal class NugetTestSetup(IMsBuildProjectHelper msBuildProjectHelper, INugetTempEnvironmentManager nugetTempEnvironmentManager) : INugetTestSetup
    {
        public string? NugetCommandPath { get; set; }

        public void Setup(string nupkgPath, XDocument project)
        {
            nugetTempEnvironmentManager.Setup(nupkgPath, project, "$(BaseIntermediateOutputPath)\\packages", NugetCommandPath);
            msBuildProjectHelper.AddPackageReference(project, nupkgPath);
        }

        public void TearDown() => nugetTempEnvironmentManager.CleanUp();
    }
}