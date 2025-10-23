using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    internal sealed class NugetAddCommand : INugetAddCommand
    {
        public Task<ProcessResult> AddPackageToSourceAsync(string nupkgPath, string source, string? nugetCommandPath)
        {
            string fileName = nugetCommandPath ?? "nuget";
            string args = $"add \"{nupkgPath}\" -Source \"{source}\"";
            return ProcessHelper.StartAndWaitAsync(fileName, args, Directory.GetCurrentDirectory());
        }
    }
}
