using System.Globalization;
using ExolvraTestApp;
using Xunit;

namespace ExolvraTestApp.Tests;

public sealed class PasswordCliServiceTests
{
    [Fact]
    public void WritePasswords_WhenQuiet_WritesOnlyGeneratedPasswordLines()
    {
        var output = new StringWriter(CultureInfo.InvariantCulture);
        var service = new PasswordCliService(new StubPasswordGenerator("abcD1234", "wxyz9876"));

        service.WritePasswords(new PasswordCliRequest(8, 2, Quiet: true), output);

        string rendered = output.ToString();
        string[] lines = rendered.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(new[] { "abcD1234", "wxyz9876" }, lines);
        Assert.DoesNotContain("Password", rendered);
        Assert.DoesNotContain("ExolvraTestApp", rendered);
    }

    private sealed class StubPasswordGenerator : IPasswordGenerator
    {
        private readonly Queue<string> _passwords;

        public StubPasswordGenerator(params string[] passwords)
        {
            _passwords = new Queue<string>(passwords);
        }

        public int AlphabetSize => 1;

        public string Generate(int length)
        {
            return _passwords.Dequeue();
        }
    }
}
