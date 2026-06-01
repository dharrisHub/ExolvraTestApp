using System;
using System.IO;
using System.Linq;
using Xunit;

namespace ExolvraTestApp.Tests;

public sealed class MinDigitsOptionTests
{
    [Fact]
    public void Main_WhenMinDigitsIsPassed_GeneratesEachPasswordWithAtLeastThatManyDigits()
    {
        CliRun run = RunCli("--quiet", "--count", "20", "--length", "12", "--min-digits", "4");

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);

        string[] passwords = run.StandardOutput.Split(
            Environment.NewLine,
            StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(20, passwords.Length);
        foreach (string password in passwords)
        {
            Assert.Equal(12, password.Length);
            Assert.True(
                password.Count(char.IsAsciiDigit) >= 4,
                $"Expected at least 4 digits in '{password}'.");
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
