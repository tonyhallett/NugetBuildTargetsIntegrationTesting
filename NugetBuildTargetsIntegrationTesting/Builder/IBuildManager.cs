namespace NugetBuildTargetsIntegrationTesting.Builder
{
    internal interface IBuildManager
    {
        Task<BuildResult> BuildAsync(ProjectBuildContext projectContext, bool isDotnet, string arguments);
    }
}
