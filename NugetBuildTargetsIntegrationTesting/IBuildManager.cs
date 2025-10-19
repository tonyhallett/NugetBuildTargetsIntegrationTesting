
namespace NugetBuildTargetsIntegrationTesting
{
    internal interface IBuildManager
    {
        IBuildResult Build(ProjectBuildContext projectContext, bool isDotnet, string arguments);
    }
}
