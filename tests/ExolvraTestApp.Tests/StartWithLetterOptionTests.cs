using System;
using System.IO;
using Xunit;

namespace ExolvraTestApp.Tests;

public sealed class StartWithLetterOptionTests
{
    [Fact]
    public void Main_WhenStartWithLetterIsPassed_GeneratesEachPasswordWithAsciiLetterFirst()
    {
        CliRun run = RunCli(
            "--quiet",
            "--count",
            "50",
            "--length",
            "16",
            "--symbols",
            "--start-with-letter");

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);

        string[] passwords = run.StandardOutput.Split(
            Environment.NewLine,
            StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(50, passwords.Length);
        foreach (string password in passwords)
        {
            Assert.Equal(16, password.Length);
            Assert.True(
                char.IsAsciiLetter(password[0]),
                $"Expected first character to be an ASCII letter in '{password}'.");
        }
    }

    private static CliRun RunCli(params string[] args)
    {
        TextWriter originalOut = Console.Out;
        TextWriter originalError = Console.Error;
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();

        try
        {
            Console.SetOut(stdout);
            Console.SetError(stderr);
            int exitCode = Program.Main(args);
            return new CliRun(exitCode, stdout.ToString(), stderr.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    private sealed record CliRun(int ExitCode, string StandardOutput, string StandardError);
}
