using System.Diagnostics.CodeAnalysis;
using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting.DotNet
{
    [ExcludeFromCodeCoverage]
    internal sealed class DotNetSdk : IDotNetSdk
    {
        public string DotNetFileName { get; set; } = "dotnet";

        private async Task<string?> GetInstallDirectoryAsync(string sdkVersion)
        {
            ProcessResult result = await ProcessHelper.StartAndWaitAsync(DotNetFileName, "--list-sdks");
            if (result.ExitCode != 0)
            {
                return null;
            }

            string[] lines = result.StandardOutput.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                int openingBracketIndex = line.IndexOf('[');
                if (openingBracketIndex != -1)
                {
                    string version = line[..openingBracketIndex].Trim();
                    if (version == sdkVersion)
                    {
                        string path = line[openingBracketIndex..].Trim('[', ']');
                        string sdkPath = Path.Combine(path, version);
                        return sdkPath;
                    }
                }
            }

            return null;
        }

        private async Task<string?> GetVersionAsync()
        {
            ProcessResult result = await ProcessHelper.StartAndWaitAsync(DotNetFileName, "--version");
            return result.ExitCode != 0 ? null : result.StandardOutput.Trim();
        }

        public async Task<string?> GetActiveSdkSdksPathAsync()
        {
            string? version = await GetVersionAsync();
            return version == null ? null : await GetInstallDirectoryAsync(version);
        }
    }
}
