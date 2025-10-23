using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    internal interface INugetTempEnvironmentManager
    {
        Task SetupAsync(string nupkgPath, XDocument project, string restorePackagesPath, string? nugetCommandPath);

        void CleanUp();
    }
}
