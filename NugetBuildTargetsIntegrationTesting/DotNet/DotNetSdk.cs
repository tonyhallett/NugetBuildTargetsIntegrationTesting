using System.Diagnostics.CodeAnalysis;
using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting.DotNet
{
    [ExcludeFromCodeCoverage]
    internal sealed class DotNetSdk : IDotNetSdk
    {
        public string DotNetFileName { get; set; } = "dotnet";

        private string? GetInstallDirectory(string sdkVersion)
        {
            ProcessResult result = ProcessHelper.StartAndWait(DotNetFileName, "--list-sdks");
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

        private string? GetVersion()
        {
            ProcessResult result = ProcessHelper.StartAndWait(DotNetFileName, "--version");
            return result.ExitCode != 0 ? null : result.StandardOutput.Trim();
        }

        public string? GetActiveSdkSdksPath()
        {
            string? version = GetVersion();
            return version == null ? null : GetInstallDirectory(version);
        }
    }
}
