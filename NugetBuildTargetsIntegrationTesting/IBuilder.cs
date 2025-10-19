
namespace NugetBuildTargetsIntegrationTesting
{
    public interface IBuilder
    {
        IBuildResult DotNetBuildProject(string arguments = "");

        IBuildResult MSBuildBuildProject(string arguments = "");
    }
}
