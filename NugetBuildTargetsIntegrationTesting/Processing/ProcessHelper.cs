namespace NugetBuildTargetsIntegrationTesting.Processing
{
    internal static class ProcessHelper
    {
        public static ProcessResult StartAndWait(string fileName, string arguments, string? workingDirectory = null)
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return new ProcessResult(output, error, process.ExitCode);
        }
    }
}