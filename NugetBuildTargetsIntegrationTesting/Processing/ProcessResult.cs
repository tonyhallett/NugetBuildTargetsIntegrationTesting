namespace NugetBuildTargetsIntegrationTesting.Processing
{
    public class ProcessResult(string output, string error, int exitCode)
    {
        public string Output { get; } = output;

        public string Error { get; } = error;

        public int ExitCode { get; } = exitCode;
    }
}