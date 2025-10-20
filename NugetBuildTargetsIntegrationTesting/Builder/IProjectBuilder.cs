namespace NugetBuildTargetsIntegrationTesting.Builder
{
    public interface IProjectBuilder
    {
        IBuildResult BuildWithDotNet(string arguments = "");

        IBuildResult BuildWithMSBuild(string arguments = "");
    }
}
