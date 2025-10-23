namespace NugetBuildTargetsIntegrationTesting.Builder
{
    public interface IProjectBuilder
    {
        Task<IBuildResult> BuildWithDotNetAsync(string arguments = "");

        Task<IBuildResult> BuildWithMSBuildAsync(string arguments = "");
    }
}
