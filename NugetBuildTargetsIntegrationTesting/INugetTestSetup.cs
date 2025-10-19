using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting
{
    public interface INugetTestSetup
    {
        string? NugetCommandPath { get; set; }

        void Setup(string nupkgPath, XDocument project);

        void TearDown();
    }
}
