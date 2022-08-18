using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Gameframe.Shell
{
  public static class ShellUtility
  {
    /// <summary>
    /// Executes a command in the system shell.
    /// Windows uses: powershell.exe by default. Use ExecuteWindowsCommand for cmd.exe
    /// Mac uses: /bin/bash
    /// </summary>
    /// <param name="command">Command to execute</param>
    /// <param name="useShell">If true we'll use the OS shell. Otherwise command will execute as its own process.</param>
    /// <returns>True if command exited with code 0 otherwise returns false</returns>
    public static bool ExecuteCommand(string command, bool useShell = false)
    {
#if UNITY_EDITOR_WIN
      var commandBytes = System.Text.Encoding.Unicode.GetBytes(command);
      var encodedCommand = Convert.ToBase64String(commandBytes);
      var processInfo = new ProcessStartInfo("powershell.exe", $"-EncodedCommand {encodedCommand}")
      {
        CreateNoWindow = true,
        UseShellExecute = useShell,
        RedirectStandardOutput = !useShell,
        WindowStyle = ProcessWindowStyle.Hidden
      };
#else
      var processInfo = new ProcessStartInfo("/bin/bash", $"-c \"{command.Replace("\\","\\\\")}\"")
      {
          CreateNoWindow = true,
          UseShellExecute = useShell,
          RedirectStandardOutput = !useShell,
          WindowStyle = ProcessWindowStyle.Hidden
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
    /// Execute command using the basic windows shell cmd.exe
    /// </summary>
    /// <param name="command">command to be executed</param>
    /// <returns>true if successfull. false otherwise.</returns>
    public static bool ExecuteWindowsCommand(string command)
    {
      command = $"/c {command}";
      var processInfo = new ProcessStartInfo("cmd.exe", command.Replace("\\","\\\\"))
      {
          CreateNoWindow = true,
          Verb = "runas",
          WindowStyle = ProcessWindowStyle.Hidden
      };

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
    /// <param name="command">Command string to be run</param>
    /// <param name="trimOutput">True if we should trim new line and line feed from end of output stream</param>
    /// <returns>String output of the command.</returns>
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
      var processInfo = new ProcessStartInfo("/bin/bash", $"-c \"{command.Replace("\\","\\\\")}\"")
      {
          CreateNoWindow = true,
          UseShellExecute = false,
          RedirectStandardOutput = true,
          WindowStyle = ProcessWindowStyle.Hidden
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
    /// <param name="trimOutput">Trims any new line characters from the end of the output</param>
    /// <returns>Task that results in a string containing the output of the command. OR null if command failed to execute.</returns>
    public static async Task<string> GetCommandResultAsync(string command, bool trimOutput = true)
    {
      var task = Task.Run(() => GetCommandResult(command,trimOutput));
      await task;
      return task.Result;
    }

    /// <summary>
    /// Execute a shell command as a task
    /// </summary>
    /// <param name="command">command string to execute</param>
    /// <param name="useShell">If true the command will execute in a system shell.</param>
    /// <returns>Task with a result of true when command exits with code 0.</returns>
    public static async Task<bool> ExecuteCommandAsync(string command, bool useShell = false)
    {
      var task = Task.Run(() => ExecuteCommand(command, useShell));
      await task;
      return task.Result;
    }

    /// <summary>
    /// Executes a command to create a symbolic link
    /// </summary>
    /// <param name="source">Source file path</param>
    /// <param name="destination">Destination file path</param>
    /// <param name="directory">set this to true if you're creating a link for a directory</param>
    /// <returns>true if the command is a success. false on failure.</returns>
    public static bool CreateSymbolicLink(string source, string destination)
    {
#if UNITY_EDITOR_WIN
      return ExecuteWindowsCommand($"mklink /D \"{destination}\" \"{source}\"");
#else
      return ExecuteCommand($"ln -s '{source}' '{destination}'");
#endif
    }

  }
}
