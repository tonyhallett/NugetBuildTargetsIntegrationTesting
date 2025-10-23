using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    internal interface INugetAddCommand
    {
        Task<ProcessResult> AddPackageToSourceAsync(string nupkgPath, string source, string? nugetCommandPath);
    }
}
