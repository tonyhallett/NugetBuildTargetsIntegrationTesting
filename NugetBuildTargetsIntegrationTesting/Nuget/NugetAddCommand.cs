using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    internal class NugetAddCommand : INugetAddCommand
    {
        public ProcessResult AddPackageToSource(string nupkgPath, string source, string? nugetCommandPath)
        {
            var fileName = nugetCommandPath ?? "nuget";
            var args = $"add \"{nupkgPath}\" -Source \"{source}\"";
            return ProcessHelper.StartAndWait(fileName, args, Directory.GetCurrentDirectory());
        }
    }
}