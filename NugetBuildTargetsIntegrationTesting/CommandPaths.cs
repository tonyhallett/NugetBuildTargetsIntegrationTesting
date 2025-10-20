
namespace NugetBuildTargetsIntegrationTesting
{
    public class CommandPaths(string? nuget, string? msBuild, string? dotNet)
    {
        public string? Nuget { get; } = nuget;

        public string? MsBuild { get; } = msBuild;
        
        public string? DotNet { get; } = dotNet;
    }
}
