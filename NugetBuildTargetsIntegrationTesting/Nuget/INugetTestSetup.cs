using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    internal interface INugetTestSetup
    {
        string? NugetCommandPath { get; set; }

        Task SetupAsync(string nupkgPath, XDocument project);

        void TearDown();
    }
}
