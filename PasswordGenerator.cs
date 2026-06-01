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
    private readonly string _letterAlphabet;

    public PasswordGenerator(Charset charset)
    {
        _alphabet = BuildAlphabet(charset);
        _digitAlphabet = BuildDigitAlphabet(charset);
        _letterAlphabet = BuildLetterAlphabet(charset);
    }

    public int AlphabetSize => _alphabet.Length;

    public int DigitAlphabetSize => _digitAlphabet.Length;

    public int LetterAlphabetSize => _letterAlphabet.Length;

    public string Generate(int length, int minDigits = 0, bool startWithLetter = false)
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
        if (startWithLetter && _letterAlphabet.Length == 0)
        {
            throw new InvalidOperationException("letter alphabet is empty — cannot satisfy startWithLetter");
        }
        if (startWithLetter && minDigits >= length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minDigits),
                minDigits,
                "minDigits must leave room for the required starting letter");
        }

        var chars = new char[length];
        int firstRandomIndex = 0;
        if (startWithLetter)
        {
            chars[0] = _letterAlphabet[RandomNumberGenerator.GetInt32(_letterAlphabet.Length)];
            firstRandomIndex = 1;
        }

        for (int i = firstRandomIndex; i < firstRandomIndex + minDigits; i++)
        {
            chars[i] = _digitAlphabet[RandomNumberGenerator.GetInt32(_digitAlphabet.Length)];
        }

        for (int i = firstRandomIndex + minDigits; i < length; i++)
        {
            chars[i] = _alphabet[RandomNumberGenerator.GetInt32(_alphabet.Length)];
        }

        Shuffle(chars, firstRandomIndex);
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

    private static string BuildLetterAlphabet(Charset c)
    {
        var sb = new StringBuilder();
        if (c.IncludeLower) sb.Append(LowercaseChars);
        if (c.IncludeUpper) sb.Append(UppercaseChars);

        return ApplyFilters(sb.ToString(), c.ExcludeSimilar, c.ExcludeAmbiguous, c.ExcludedChars);
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

    private static void Shuffle(char[] chars, int startIndex = 0)
    {
        for (int i = chars.Length - 1; i > startIndex; i--)
        {
            int j = RandomNumberGenerator.GetInt32(startIndex, i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
    }
}
