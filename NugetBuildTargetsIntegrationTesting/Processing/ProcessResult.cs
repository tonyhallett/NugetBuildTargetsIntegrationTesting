namespace NugetBuildTargetsIntegrationTesting.Processing
{
    internal sealed class ProcessResult(string output, string error, int exitCode)
    {
        public string StandardOutput { get; } = output;

        public string StandardError { get; } = error;

        public int ExitCode { get; } = exitCode;
    }
}
