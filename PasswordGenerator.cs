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
    public const string SimilarChars = "lI1O0";
    public const string AmbiguousChars = "0O1lI|`'\"";

    public sealed record Charset(
        bool IncludeLower = true,
        bool IncludeUpper = true,
        bool IncludeDigits = true,
        bool IncludeSymbols = false,
        bool ExcludeSimilar = false,
        bool ExcludeAmbiguous = false,
        string ExcludedChars = "");

    private readonly string _alphabet;
    private readonly string _digitAlphabet;

    public PasswordGenerator(Charset charset)
    {
        _alphabet = BuildAlphabet(charset);
        _digitAlphabet = BuildDigitAlphabet(charset);
    }

    public int AlphabetSize => _alphabet.Length;

    public int DigitAlphabetSize => _digitAlphabet.Length;

    public string Generate(int length, int minDigits = 0)
    {
        if (length < MinLength || length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                length,
                $"length must be between {MinLength} and {MaxLength}");
        }
        if (minDigits < 0 || minDigits > length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minDigits),
                minDigits,
                $"minDigits must be between 0 and the requested length");
        }
        if (_alphabet.Length == 0)
        {
            throw new InvalidOperationException("alphabet is empty — no character classes enabled");
        }
        if (minDigits > 0 && _digitAlphabet.Length == 0)
        {
            throw new InvalidOperationException("digit alphabet is empty — cannot satisfy minDigits");
        }

        var chars = new char[length];
        for (int i = 0; i < minDigits; i++)
        {
            chars[i] = _digitAlphabet[RandomNumberGenerator.GetInt32(_digitAlphabet.Length)];
        }

        for (int i = minDigits; i < length; i++)
        {
            chars[i] = _alphabet[RandomNumberGenerator.GetInt32(_alphabet.Length)];
        }

        Shuffle(chars);
        return new string(chars);
    }

    private static string BuildAlphabet(Charset c)
    {
        var sb = new StringBuilder();
        if (c.IncludeLower) sb.Append(LowercaseChars);
        if (c.IncludeUpper) sb.Append(UppercaseChars);
        if (c.IncludeDigits) sb.Append(DigitChars);
        if (c.IncludeSymbols) sb.Append(SymbolChars);

        return ApplyFilters(sb.ToString(), c.ExcludeSimilar, c.ExcludeAmbiguous, c.ExcludedChars);
    }

    private static string BuildDigitAlphabet(Charset c)
    {
        return c.IncludeDigits
            ? ApplyFilters(DigitChars, c.ExcludeSimilar, c.ExcludeAmbiguous, c.ExcludedChars)
            : string.Empty;
    }

    private static string ApplyFilters(string chars, bool excludeSimilar, bool excludeAmbiguous, string excludedChars)
    {
        if (!excludeSimilar && !excludeAmbiguous && string.IsNullOrEmpty(excludedChars)) return chars;

        var filtered = new StringBuilder(chars.Length);
        foreach (char ch in chars)
        {
            if (excludeSimilar && SimilarChars.IndexOf(ch) >= 0) continue;
            if (excludeAmbiguous && AmbiguousChars.IndexOf(ch) >= 0) continue;
            if (!string.IsNullOrEmpty(excludedChars) && excludedChars.IndexOf(ch) >= 0) continue;

            filtered.Append(ch);
        }
        return filtered.ToString();
    }

    private static void Shuffle(char[] chars)
    {
        for (int i = chars.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
    }
}
