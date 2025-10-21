using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting.DotNet
{
    internal class DotNetSdk : IDotNetSdk
    {
        private const string dotnetFileName = "dotnet";

        private static string? GetInstallDirectory(string sdkVersion)
        {
            var result = ProcessHelper.StartAndWait(dotnetFileName, $"--list-sdks");
            if (result.ExitCode != 0)
            {
                return null;
            }
            var lines = result.Output.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var openingBracketIndex = line.IndexOf('[');
                if (openingBracketIndex != -1)
                {
                    var version = line.Substring(0, openingBracketIndex).Trim();
                    if (version == sdkVersion)
                    {
                        var path = line.Substring(openingBracketIndex).Trim('[', ']');
                        var sdkPath = Path.Combine(path, version);
                        return sdkPath;
                    }
                }
            }
            return null;
        }

        private static string? GetVersion()
        {
            var result = ProcessHelper.StartAndWait(dotnetFileName, "--version");
            if (result.ExitCode != 0)
            {
                return null;
            }
            return result.Output.Trim();
        }

        public string? GetActiveSdkSdksPath()
        {
            var version = GetVersion();
            if (version == null)
            {
                return null;
            }

            return GetInstallDirectory(version);
        }
    }
}
