using System;
using System.Globalization;
using System.IO;
using Xunit;

namespace ExolvraTestApp.Tests;

public sealed class ShowEntropyOptionTests
{
    [Fact]
    public void Main_WhenShowEntropyIsPassed_PrintsBitStrengthToStderrPlusPassword()
    {
        CliRun run = RunCli("--length", "16", "--show-entropy");

        Assert.Equal(0, run.ExitCode);

        // Default alphabet is 62 chars: 16 * log2(62) ~= 95.3 bits.
        Assert.Contains("# entropy: 95.3 bits (alphabet 62, length 16)", run.StandardError);

        string[] passwords = run.StandardOutput.Split(
            Environment.NewLine,
            StringSplitOptions.RemoveEmptyEntries);
        Assert.Single(passwords);
        Assert.Equal(16, passwords[0].Length);
    }

    [Fact]
    public void Main_WhenShowEntropyIsPassedWithSymbols_ReportsLargerAlphabet()
    {
        CliRun run = RunCli("--length", "20", "--symbols", "--show-entropy");

        Assert.Equal(0, run.ExitCode);

        // Alphabet with symbols is 88 chars (62 + 26 symbols): 20 * log2(88) ~= 129.2 bits.
        int alphabetSize = (PasswordGenerator.LowercaseChars
            + PasswordGenerator.UppercaseChars
            + PasswordGenerator.DigitChars
            + PasswordGenerator.SymbolChars).Length;
        string expected = string.Format(
            CultureInfo.InvariantCulture,
            "# entropy: {0:F1} bits (alphabet {1}, length 20)",
            20 * Math.Log2(alphabetSize),
            alphabetSize);
        Assert.Contains(expected, run.StandardError);

        string[] passwords = run.StandardOutput.Split(
            Environment.NewLine,
            StringSplitOptions.RemoveEmptyEntries);
        Assert.Single(passwords);
        Assert.Equal(20, passwords[0].Length);
    }

    [Fact]
    public void Main_WhenQuietIsCombinedWithShowEntropy_SuppressesEntropyButPrintsPassword()
    {
        CliRun run = RunCli("--length", "16", "--show-entropy", "--quiet");

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);

        string[] passwords = run.StandardOutput.Split(
            Environment.NewLine,
            StringSplitOptions.RemoveEmptyEntries);
        Assert.Single(passwords);
        Assert.Equal(16, passwords[0].Length);
    }

    [Fact]
    public void Main_WhenHelpIsRequested_DescribesShowEntropyFlag()
    {
        CliRun run = RunCli("--help");

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.Contains("--show-entropy", run.StandardOutput);
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
