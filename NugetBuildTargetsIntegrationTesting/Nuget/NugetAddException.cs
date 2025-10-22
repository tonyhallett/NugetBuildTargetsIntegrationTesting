namespace NugetBuildTargetsIntegrationTesting.Nuget
{
#pragma warning disable RCS1194 // Implement exception constructors
    public class NugetAddException(string standardError, string standardOutput, int exitCode)
#pragma warning restore RCS1194 // Implement exception constructors
        : Exception(GetMessage(standardError, standardOutput, exitCode))
    {
        private static string GetMessage(string error, string output, int exitCode)
            => $"ExitCode {exitCode}. {error} {Environment.NewLine} {output} ";

        public string StandardError { get; } = standardError;

        public string StandardOutput { get; } = standardOutput;

        public int ExitCode { get; } = exitCode;
    }
}
