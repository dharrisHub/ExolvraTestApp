using System;
using System.IO;
using Xunit;

namespace ExolvraTestApp.Tests;

public sealed class NoSimilarOptionTests
{
    [Fact]
    public void Main_WhenNoSimilarIsPassed_ExcludesLookAlikeCharacters()
    {
        CliRun run = RunCli("--quiet", "--count", "10", "--length", "64", "--no-similar");

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);

        string[] passwords = run.StandardOutput.Split(
            Environment.NewLine,
            StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(10, passwords.Length);
        foreach (string password in passwords)
        {
            Assert.Equal(64, password.Length);
            Assert.DoesNotContain(password, static ch => PasswordGenerator.SimilarChars.IndexOf(ch) >= 0);
        }
    }

    [Fact]
    public void Main_WhenNoSimilarRemovesOnlyRemainingCharacter_ReturnsEmptyAlphabetError()
    {
        CliRun run = RunCli(
            "--no-lower",
            "--no-upper",
            "--exclude-chars",
            "23456789",
            "--no-similar");

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
