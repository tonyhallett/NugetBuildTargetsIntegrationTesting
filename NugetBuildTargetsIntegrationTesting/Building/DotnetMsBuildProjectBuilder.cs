using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting.Building
{
    internal sealed class DotnetMsBuildProjectBuilder : IDotnetMsBuildProjectBuilder
    {
        private readonly string _defaultDotNetBuildArguments;
        private readonly string _noNodeReuseProperty = CreatePropertySwitch("nodeReuse", "false");
        private readonly string _defaultMSBuildArguments;
        private string _dotnetFileName = "dotnet";
        private string _msBuildFileName = "msbuild";

        public DotnetMsBuildProjectBuilder()
        {
            _defaultMSBuildArguments = $"-restore {_noNodeReuseProperty} {CreatePropertySwitch("Configuration", "Release")}";
            _defaultDotNetBuildArguments = $"-c Release {_noNodeReuseProperty}";
        }

        private static string CreatePropertySwitch(string name, string value) => $"-property:{name}={value}";

        public Task<ProcessResult> BuildAsync(string projectFilePath, bool isDotnet, string arguments, string workingDirectory)
        {
            projectFilePath = QuotePath(projectFilePath);
            return isDotnet
                ? DotNetBuildAsync(projectFilePath, arguments, workingDirectory)
                : MSBuildBuildAsync(projectFilePath, arguments, workingDirectory);
        }

        private Task<ProcessResult> MSBuildBuildAsync(string quotedProjectFilePath, string arguments, string workingDirectory)
        {
            arguments = arguments.Length == 0 ? _defaultMSBuildArguments : arguments;
            return ProcessHelper.StartAndWaitAsync(
                _msBuildFileName,
                $"{QuotePath(quotedProjectFilePath)} {arguments}",
                workingDirectory);
        }

        private static string QuotePath(string path) => $"\"{path}\"";

        private Task<ProcessResult> DotNetBuildAsync(string quotedProjectFilePath, string arguments, string workingDirectory)
        {
            arguments = arguments.Length == 0 ? _defaultDotNetBuildArguments : arguments;
            arguments = $"build {quotedProjectFilePath} {arguments}";
            return ProcessHelper.StartAndWaitAsync(_dotnetFileName, arguments, workingDirectory);
        }

        public void SetCommandPaths(string? dotNet, string? msBuild)
        {
            if (dotNet != null)
            {
                _dotnetFileName = dotNet;
            }

            if (msBuild == null)
            {
                return;
            }

            _msBuildFileName = msBuild;
        }
    }
}
