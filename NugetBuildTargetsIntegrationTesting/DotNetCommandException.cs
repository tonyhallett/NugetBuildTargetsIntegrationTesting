namespace NugetBuildTargetsIntegrationTesting
{
    public class DotNetCommandException(string command, string standardOutput, string standardError, int exitCode) : Exception(GetMessage(command, standardOutput, standardError, exitCode))
    {
        public string Command { get; } = command;

        public string StandardOutput { get; } = standardOutput;

        public string StandardError { get; } = standardError;

        public int ExitCode { get; } = exitCode;

        private static string GetMessage(string command, string standardOutput, string standardError, int exitCode)
        {
            return $"Command '{command}' failed with exit code {exitCode}.\nStandard Output: {standardOutput}\nStandard Error: {standardError}";
        }
    }
}