namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    internal sealed class NugetGlobalPackagesPathProvider : INuGetGlobalPackagesPathProvider
    {
        public string Provide() => Environment.GetEnvironmentVariable("NUGET_PACKAGES")
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
    }
}
