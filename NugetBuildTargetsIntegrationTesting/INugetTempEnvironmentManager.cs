using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting
{
    public interface INugetTempEnvironmentManager
    {
        void Setup(string nupkgPath, XDocument project, string restorePackagesPath);

        void CleanUp();
    }
}