using System.Xml.Linq;
using NugetBuildTargetsIntegrationTesting.MSBuildHelpers;

namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    internal sealed class NugetTestSetup(
        IMsBuildProjectHelper msBuildProjectHelper,
        INugetTempEnvironmentManager nugetTempEnvironmentManager)
        : INugetTestSetup
    {
        public string? NugetCommandPath { get; set; }

        public async Task SetupAsync(string nupkgPath, XDocument project)
        {
            await nugetTempEnvironmentManager.SetupAsync(nupkgPath, project, "$(BaseIntermediateOutputPath)\\packages", NugetCommandPath);
            msBuildProjectHelper.AddPackageReference(project, nupkgPath);
        }

        public void TearDown() => nugetTempEnvironmentManager.CleanUp();
    }
}
