using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting
{
    public interface INugetTestSetup
    {
        void Setup(string nupkgPath, XDocument project);

        void TearDown();
    }
}
