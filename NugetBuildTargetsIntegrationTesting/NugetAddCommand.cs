using System.Diagnostics;

namespace NugetBuildTargetsIntegrationTesting
{
    internal class NugetAddCommand : INugetAddCommand
    {
        private static void RunCommand(string fileName, string args, string workingDir)
        {
            var proc = Process.Start(new ProcessStartInfo(fileName, args)
            {
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });
            proc!.WaitForExit();
            if (proc.ExitCode != 0)
                throw new Exception($"Command failed: {fileName} {args}\n{proc.StandardOutput.ReadToEnd()}\n{proc.StandardError.ReadToEnd()}");
        }

        public void AddPackageToSource(string nupkgPath, string source, string? nugetCommandPath)
        {
            var fileName = nugetCommandPath ?? "nuget";
            var args = $"add \"{nupkgPath}\" -Source \"{source}\"";
            RunCommand(fileName, args, Directory.GetCurrentDirectory());
        }
    }
}