namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    internal class NugetGlobalPackagesPathProvider : INuGetGlobalPackagesPathProvider
    {
        public string Provide() => Environment.GetEnvironmentVariable("NUGET_PACKAGES")
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
    }
}
