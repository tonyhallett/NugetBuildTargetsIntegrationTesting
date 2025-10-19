
namespace NugetBuildTargetsIntegrationTesting
{
    interface IDotnetMsBuildProjectBuilder
    {
        ProcessResult Build(string projectFilePath, bool isDotnet, string arguments, string workingDirectory);

        void SetCommandPaths(string? dotNet, string? msBuild);
    }
}
