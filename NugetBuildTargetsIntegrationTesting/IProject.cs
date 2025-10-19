
namespace NugetBuildTargetsIntegrationTesting
{
    public interface IProject
    {
        IProject AddFiles(List<(string Contents, string RelativePath)> files);
        IAddNuget AddProject(string projectContents, string relativePath = "dependentProject.csproj");
    }
}
