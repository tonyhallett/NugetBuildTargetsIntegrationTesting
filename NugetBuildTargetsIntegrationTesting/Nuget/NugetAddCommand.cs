using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    internal sealed class NugetAddCommand : INugetAddCommand
    {
        public ProcessResult AddPackageToSource(string nupkgPath, string source, string? nugetCommandPath)
        {
            string fileName = nugetCommandPath ?? "nuget";
            string args = $"add \"{nupkgPath}\" -Source \"{source}\"";
            return ProcessHelper.StartAndWait(fileName, args, Directory.GetCurrentDirectory());
        }
    }
}
