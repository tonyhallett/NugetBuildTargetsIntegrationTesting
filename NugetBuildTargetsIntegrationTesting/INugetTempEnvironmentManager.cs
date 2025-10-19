using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting
{
    public interface INugetTempEnvironmentManager
    {
        void Setup(string nupkgPath, XDocument project, string restorePackagesPath, string? nugetCommandPath);

        void CleanUp();
    }
}