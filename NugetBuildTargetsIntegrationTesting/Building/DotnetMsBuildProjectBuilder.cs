using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting.Building
{
    internal class DotnetMsBuildProjectBuilder : IDotnetMsBuildProjectBuilder
    {
        private const string defaultDotNetBuildArguments = "-c Release";

        private string dotnetFileName = "dotnet";
        private string msBuildFileName = "msbuild";

        public ProcessResult Build(string projectFilePath, bool isDotnet, string arguments, string workingDirectory)
        {
            if (isDotnet)
            {
                arguments = arguments == string.Empty ? defaultDotNetBuildArguments : arguments;
                return DotNetBuild(projectFilePath, arguments, workingDirectory);
            }

            throw new NotImplementedException();
        }

        public ProcessResult DotNetBuild(string projectPath, string arguments, string workingDirectory)
            => DotNetCommand("build", projectPath, workingDirectory, arguments);

        private ProcessResult DotNetCommand(string command, string projectPath, string workingDirectory, string additionalArguments = "")
        {
            var arguments = $"{command} \"{projectPath}\" {additionalArguments}";
            return ProcessHelper.StartAndWait(dotnetFileName, arguments, workingDirectory);
        }

        public void SetCommandPaths(string? dotNet, string? msBuild)
        {
            if (dotNet != null)
            {
                dotnetFileName = dotNet;
            }

            if (msBuild != null)
            {
                msBuildFileName = msBuild;
            }
        }
    }
}
