using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gameframe.Shell.Tests
{
    public class ShellUtilityTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void ExecuteCommand_UseShell([Values(true,false)] bool useShell)
        {
            var command = "echo \"hello\"";
            var result = ShellUtility.ExecuteCommand(command,useShell);
            Assert.IsTrue(result,$"Failed to execute command: {command}");
        }
        
        [Test]
        public void GetCommandResult_TrimOutput([Values(true,false)] bool trimOutput)
        {
            var echoText = "hello";
            var command = $"echo \"{echoText}\"";
            var result = ShellUtility.GetCommandResult(command,trimOutput);

            if (trimOutput)
            {
                Assert.IsTrue(result == echoText,$"Failed to execute command and get result: {echoText} != {result}");
            }
            else
            {
                Assert.IsTrue(result != echoText,$"Failed to execute command and get result: {echoText} != {result}");
                Assert.IsTrue(result.StartsWith(echoText),$"Failed to execute command and get result: {echoText} != {result}");
            }
        }
        
        [UnityTest]
        public IEnumerator ExecuteCommandAsync_UseShell([Values(true,false)] bool useShell)
        {
            var command = "echo \"hello\"";
            var task = ShellUtility.ExecuteCommandAsync(command,useShell);

            while (!task.IsCompleted)
            {
                yield return null;
            }

            var result = task.Result;
            Assert.IsTrue(result,$"Failed to execute command: {command}");
        }
        
        [UnityTest]
        public IEnumerator GetCommandResultAsync_TrimOutput([Values(true,false)] bool trimOutput)
        {
            var echoText = "hello";
            var command = $"echo \"{echoText}\"";
            var task = ShellUtility.GetCommandResultAsync(command,trimOutput);

            while (!task.IsCompleted)
            {
                yield return null;
            }

            var result = task.Result;
            
            if (trimOutput)
            {
                Assert.IsTrue(result == echoText,$"Failed to execute command and get result: {echoText} != {result}");
            }
            else
            {
                Assert.IsTrue(result != echoText,$"Failed to execute command and get result: {echoText} != {result}");
                Assert.IsTrue(result.StartsWith(echoText),$"Failed to execute command and get result: {echoText} != {result}");
            }
        }
    }
}
