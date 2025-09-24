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
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"{command} \"{projectPath}\" {additionalArguments}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new DotNetCommandException(command, output,error,process.ExitCode);
            }
            return output;
        }
    }
}