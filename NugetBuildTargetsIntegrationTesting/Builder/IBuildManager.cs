
namespace NugetBuildTargetsIntegrationTesting.Builder
{
    internal interface IBuildManager
    {
        BuildResult Build(ProjectBuildContext projectContext, bool isDotnet, string arguments);
    }
}
