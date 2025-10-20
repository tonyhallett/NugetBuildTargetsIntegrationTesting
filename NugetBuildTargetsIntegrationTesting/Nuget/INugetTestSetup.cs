using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    internal interface INugetTestSetup
    {
        string? NugetCommandPath { get; set; }

        void Setup(string nupkgPath, XDocument project);

        void TearDown();
    }
}
