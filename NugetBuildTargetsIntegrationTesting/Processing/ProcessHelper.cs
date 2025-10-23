namespace NugetBuildTargetsIntegrationTesting.Processing
{
    internal static class ProcessHelper
    {
        public static async Task<ProcessResult> StartAndWaitAsync(string fileName, string arguments, string? workingDirectory = null)
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
                    WorkingDirectory = workingDirectory,
                },
            };
            _ = process.Start();

            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
            Task<string> errorTask = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(60000))
            {
                process.Kill(entireProcessTree: true);
                return new ProcessResult(outputTask.Result, errorTask.Result, -1);
            }

            _ = await Task.WhenAll(outputTask, errorTask).ConfigureAwait(false);

            return new ProcessResult(outputTask.Result, errorTask.Result, process.ExitCode);
        }
    }
}
