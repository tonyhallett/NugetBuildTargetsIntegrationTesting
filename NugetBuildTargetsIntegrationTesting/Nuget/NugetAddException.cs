namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    public class NugetAddException(string error, string output, int exitCode) : Exception(GetMessage(error, output, exitCode))
    {
        private static string GetMessage(string error, string output, int exitCode)
            => $"ExitCode {exitCode}. {error} {Environment.NewLine} {output} ";

        public string Error { get; } = error;

        public string Output { get; } = output;

        public int ExitCode { get; } = exitCode;
    }
}