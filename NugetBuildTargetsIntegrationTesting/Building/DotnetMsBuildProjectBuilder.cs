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

        public ProcessResult Build(string projectFilePath, bool isDotnet, string arguments, string workingDirectory)
        {
            projectFilePath = QuotePath(projectFilePath);
            return isDotnet
                ? DotNetBuild(projectFilePath, arguments, workingDirectory)
                : MSBuildBuild(projectFilePath, arguments, workingDirectory);
        }

        private ProcessResult MSBuildBuild(string quotedProjectFilePath, string arguments, string workingDirectory)
        {
            arguments = arguments.Length == 0 ? _defaultMSBuildArguments : arguments;
            return ProcessHelper.StartAndWait(
                _msBuildFileName,
                $"{QuotePath(quotedProjectFilePath)} {arguments}",
                workingDirectory);
        }

        private static string QuotePath(string path) => $"\"{path}\"";

        private ProcessResult DotNetBuild(string quotedProjectFilePath, string arguments, string workingDirectory)
        {
            arguments = arguments.Length == 0 ? _defaultDotNetBuildArguments : arguments;
            arguments = $"build {quotedProjectFilePath} {arguments}";
            return ProcessHelper.StartAndWait(_dotnetFileName, arguments, workingDirectory);
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
