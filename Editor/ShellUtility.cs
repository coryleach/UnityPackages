using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Gameframe.Shell
{
  public static class ShellUtility
  {
    /// <summary>
    /// Executes a command in the system shell.
    /// windows uses: powershell.exe
    /// mac uses: /bin/bash
    /// </summary>
    /// <param name="command">command to execute</param>
    /// <param name="useShell">if true we'll use the OS shell. Otherwise command will execute as its own process.</param>
    /// <returns>true if command exited with code 0 otherwise returns false</returns>
    public static bool ExecuteCommand(string command, bool useShell = false)
    {
      
#if UNITY_EDITOR_WIN
      var commandBytes = System.Text.Encoding.Unicode.GetBytes(command);
      var encodedCommand = Convert.ToBase64String(commandBytes);
      var processInfo = new ProcessStartInfo("powershell.exe", $"-EncodedCommand {encodedCommand}")
      {
        CreateNoWindow = true,
        UseShellExecute = useShell,
        RedirectStandardOutput = !useShell
      };
#else
      var processInfo = new ProcessStartInfo("/bin/bash", command.Replace("\\","\\\\"))
      {
          CreateNoWindow = true,
          UseShellExecute = useShell,
          RedirectStandardOutput = !useShell
      };
#endif

      var process = Process.Start(processInfo);
      if (process == null)
      {
        return false;
      }

      process.WaitForExit();
      var exitCode = process.ExitCode;
      process.Close();

      return exitCode == 0;
    }

    /// <summary>
    /// Get the string output of a command in the shell
    /// </summary>
    /// <param name="command">command string to be run</param>
    /// <param name="trimOutput">true if we should trim new line and line feed from end of output stream</param>
    /// <returns>string output of the command.</returns>
    public static string GetCommandResult(string command, bool trimOutput = true)
    {
      
#if UNITY_EDITOR_WIN
      var commandBytes = System.Text.Encoding.Unicode.GetBytes(command);
      var encodedCommand = Convert.ToBase64String(commandBytes);
      var processInfo = new ProcessStartInfo("powershell.exe", $"-EncodedCommand {encodedCommand}")
      {
        CreateNoWindow = true,
        UseShellExecute = false,
        RedirectStandardOutput = true
      };
#else
      var processInfo = new ProcessStartInfo("/bin/bash", command.Replace("\\","\\\\"))
      {
          CreateNoWindow = true,
          UseShellExecute = useShellExecute,
          RedirectStandardOutput = true
      };
#endif

      var process = Process.Start(processInfo);
      if (process == null)
      {
        return null;
      }

      var output = process.StandardOutput.ReadToEnd();
      process.WaitForExit();

      var exitCode = process.ExitCode;
      process.Close();

      if (exitCode != 0)
      {
        return null;
      }
      
      if (trimOutput)
      {
        output = output.TrimEnd('\n','\r');
      }
      
      return  output;
    }

    /// <summary>
    /// Run a command and get the string output as a task
    /// </summary>
    /// <param name="command">Command string to run.</param>
    /// <param name="useShellExecute">If true we'll use the OS shell to run the command.</param>
    /// <returns>Task that results in a string containing the output of the command. OR null if command failed to execute.</returns>
    public static async Task<string> GetCommandResultAsync(string command, bool useShellExecute = false)
    {
      var task = Task.Run(() => GetCommandResult(command,useShellExecute));
      await task;
      return task.Result;
    }

    /// <summary>
    /// Execute a shell command as a task
    /// </summary>
    /// <param name="command">command string to execute</param>
    /// <param name="useShell"></param>
    /// <returns></returns>
    public static async Task<bool> ExecuteCommandAsync(string command, bool useShell = true)
    {
      var task = Task.Run(() => ExecuteCommand(command, useShell));
      await task;
      return task.Result;
    }
    
  }
}