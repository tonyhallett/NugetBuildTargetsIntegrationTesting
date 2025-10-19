namespace NugetBuildTargetsIntegrationTesting
{

    internal sealed class DotNetProjectBuilder : IProjectBuilder
    {
        public string Build(string projectPath)
        {
            return DotNetCommand("build", projectPath, "-c Release");
        }

        private static string DotNetCommand(string command, string projectPath, string additionalArguments = "")
        {
            var arguments = $"{command} \"{projectPath}\" {additionalArguments}";
            var processResult = ProcessHelper.StartAndWait("dotnet", arguments);
            if (processResult.ExitCode != 0)
            {
                throw new DotNetCommandException(command, processResult.Output,processResult.Error,processResult.ExitCode);
            }

            return processResult.Output + "\n" + processResult.Error;
        }
    }
}