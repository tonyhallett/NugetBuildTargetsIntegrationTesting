namespace NugetBuildTargetsIntegrationTesting
{
    public interface INugetAddCommand
    {
        void AddPackageToSource(string nupkgPath, string source);
    }
}