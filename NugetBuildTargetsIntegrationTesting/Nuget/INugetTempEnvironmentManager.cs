using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    internal interface INugetTempEnvironmentManager
    {
        void Setup(string nupkgPath, XDocument project, string restorePackagesPath, string? nugetCommandPath);

        void CleanUp();
    }
}
