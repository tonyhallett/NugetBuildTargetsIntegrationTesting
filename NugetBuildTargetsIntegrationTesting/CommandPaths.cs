
namespace NugetBuildTargetsIntegrationTesting
{
    public class CommandPaths
    {
        public CommandPaths(string? nuget, string? msBuild, string? dotNet)
        {
            Nuget = nuget;
            MsBuild = msBuild;
            DotNet = dotNet;
        }

        public string? Nuget { get; }
        public string? MsBuild { get; }
        public string? DotNet { get; }
    }
}
