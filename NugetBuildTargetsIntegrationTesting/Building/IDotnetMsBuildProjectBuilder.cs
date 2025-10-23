using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting.Building
{
    internal interface IDotnetMsBuildProjectBuilder
    {
        Task<ProcessResult> BuildAsync(string projectFilePath, bool isDotnet, string arguments, string workingDirectory);

        void SetCommandPaths(string? dotNet, string? msBuild);
    }
}
