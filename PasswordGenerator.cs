using System.Security.Cryptography;
using System.Text;

namespace ExolvraTestApp;

public sealed class PasswordGenerator
{
    public const int MinLength = 4;
    public const int MaxLength = 1024;

    public const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    public const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string DigitChars = "0123456789";
    public const string SymbolChars = "!@#$%^&*()-_=+[]{};:,.<>/?";
    public const string AmbiguousChars = "0O1lI|`'\"";

    public sealed record Charset(
        bool IncludeLower = true,
        bool IncludeUpper = true,
        bool IncludeDigits = true,
        bool IncludeSymbols = false,
        bool ExcludeAmbiguous = false);

    private readonly string _alphabet;

    public PasswordGenerator(Charset charset)
    {
        _alphabet = BuildAlphabet(charset);
    }

    public int AlphabetSize => _alphabet.Length;

    public string Generate(int length)
    {
        if (length < MinLength || length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                length,
                $"length must be between {MinLength} and {MaxLength}");
        }
        if (_alphabet.Length == 0)
        {
            throw new InvalidOperationException("alphabet is empty — no character classes enabled");
        }

        var chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = _alphabet[RandomNumberGenerator.GetInt32(_alphabet.Length)];
        }
        return new string(chars);
    }

    private static string BuildAlphabet(Charset c)
    {
        var sb = new StringBuilder();
        if (c.IncludeLower) sb.Append(LowercaseChars);
        if (c.IncludeUpper) sb.Append(UppercaseChars);
        if (c.IncludeDigits) sb.Append(DigitChars);
        if (c.IncludeSymbols) sb.Append(SymbolChars);

        if (!c.ExcludeAmbiguous) return sb.ToString();

        var filtered = new StringBuilder(sb.Length);
        foreach (char ch in sb.ToString())
        {
            if (AmbiguousChars.IndexOf(ch) < 0) filtered.Append(ch);
        }
        return filtered.ToString();
    }
}
