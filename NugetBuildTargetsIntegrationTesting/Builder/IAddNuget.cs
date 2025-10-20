namespace NugetBuildTargetsIntegrationTesting.Builder
{
    public interface IAddNuget
    {
        IProjectBuilder AddNuPkg(string nuPkgPath);
    }
}
