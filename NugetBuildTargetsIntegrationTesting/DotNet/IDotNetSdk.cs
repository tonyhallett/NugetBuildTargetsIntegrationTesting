namespace NugetBuildTargetsIntegrationTesting.DotNet
{
    internal interface IDotNetSdk
    {
        string DotNetFileName { get; set; }

        string? GetActiveSdkSdksPath();
    }
}
