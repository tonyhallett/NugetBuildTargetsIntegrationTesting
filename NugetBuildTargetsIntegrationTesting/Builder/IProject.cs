namespace NugetBuildTargetsIntegrationTesting.Builder
{
    public interface IProject
    {
        IProject AddFiles(IEnumerable<(string Contents, string RelativePath)> files);

        IAddNuget AddProject(string projectContents);

        IAddNuget AddProject(string projectContents, string relativePath);
    }
}
