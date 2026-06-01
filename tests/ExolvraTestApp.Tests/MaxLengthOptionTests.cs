using System;
using System.IO;
using Xunit;

namespace ExolvraTestApp.Tests;

public sealed class MaxLengthOptionTests
{
    [Fact]
    public void Main_WhenMaxLengthIsLessThanRequestedLength_CapsGeneratedPasswordLength()
    {
        CliRun run = RunCli("--quiet", "--count", "10", "--length", "24", "--max-length", "12");

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);

        string[] passwords = run.StandardOutput.Split(
            Environment.NewLine,
            StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(10, passwords.Length);
        Assert.All(passwords, static password => Assert.Equal(12, password.Length));
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
