using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    internal interface INugetAddCommand
    {
        ProcessResult AddPackageToSource(string nupkgPath, string source, string? nugetCommandPath);
    }
}