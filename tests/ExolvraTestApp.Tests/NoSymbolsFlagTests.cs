using System;
using System.IO;
using Xunit;

namespace ExolvraTestApp.Tests;

public sealed class NoSymbolsFlagTests
{
    [Fact]
    public void Main_WhenNoSymbolsIsPassedAfterSymbols_UsesOnlyLettersAndDigits()
    {
        CliRun run = RunCli("--symbols", "--no-symbols", "--quiet", "--count", "5", "--length", "64");

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);

        string[] passwords = run.StandardOutput.Split(
            Environment.NewLine,
            StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(5, passwords.Length);
        foreach (string password in passwords)
        {
            Assert.Equal(64, password.Length);
            Assert.All(password, static ch => Assert.True(
                char.IsAsciiLetterOrDigit(ch),
                $"Expected only letters and digits, but found '{ch}'."));
        }
    }

    [Fact]
    public void Main_WhenNoSymbolsRemovesOnlyEnabledClass_ReturnsEmptyAlphabetError()
    {
        CliRun run = RunCli("--no-lower", "--no-upper", "--no-digits", "--symbols", "--no-symbols");

        Assert.Equal(2, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Contains("resulting alphabet is empty", run.StandardError);
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
