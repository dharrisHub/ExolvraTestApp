using System;
using Xunit;

namespace ExolvraTestApp.Tests;

public class ParseArgsTests
{
    [Fact]
    public void LengthInline_ParsesValue()
    {
        var o = Program.ParseArgs(new[] { "--length=32" });
        Assert.Equal(32, o.Length);
    }

    [Fact]
    public void LengthInline_MatchesSpaceSeparatedAndShortForms()
    {
        int inline = Program.ParseArgs(new[] { "--length=32" }).Length;
        int spaced = Program.ParseArgs(new[] { "--length", "32" }).Length;
        int shortForm = Program.ParseArgs(new[] { "-l", "32" }).Length;

        Assert.Equal(inline, spaced);
        Assert.Equal(inline, shortForm);
    }

    [Fact]
    public void CountInline_ParsesValue()
    {
        var o = Program.ParseArgs(new[] { "--count=5" });
        Assert.Equal(5, o.Count);
    }

    [Fact]
    public void ExcludeCharsInline_ParsesValue()
    {
        var o = Program.ParseArgs(new[] { "--exclude-chars=abc" });
        Assert.Equal("abc", o.ExcludedChars);
    }

    [Fact]
    public void ExcludeCharsInline_SplitsOnlyOnFirstEquals()
    {
        var o = Program.ParseArgs(new[] { "--exclude-chars=a=b" });
        Assert.Equal("a=b", o.ExcludedChars);
    }

    [Fact]
    public void ExcludeCharsInline_EmptyValueIsAllowed()
    {
        var o = Program.ParseArgs(new[] { "--exclude-chars=" });
        Assert.Equal(string.Empty, o.ExcludedChars);
    }

    [Theory]
    [InlineData("--length=")]
    [InlineData("--length=x")]
    public void LengthInline_NonNumericThrows(string arg)
    {
        Assert.Throws<ArgumentException>(() => Program.ParseArgs(new[] { arg }));
    }

    [Fact]
    public void UnknownInlineOption_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => Program.ParseArgs(new[] { "--foo=bar" }));
        Assert.Contains("--foo=bar", ex.Message);
    }

    [Fact]
    public void BooleanFlagWithValue_Throws()
    {
        // `--symbols` takes no value, so `--symbols=true` is not split and errors as unknown.
        Assert.Throws<ArgumentException>(() => Program.ParseArgs(new[] { "--symbols=true" }));
    }

    [Fact]
    public void SpaceSeparatedForms_StillParse()
    {
        var o = Program.ParseArgs(new[] { "--length", "12", "--count", "3", "--exclude-chars", "xy" });
        Assert.Equal(12, o.Length);
        Assert.Equal(3, o.Count);
        Assert.Equal("xy", o.ExcludedChars);
    }
}
