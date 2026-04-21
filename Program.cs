using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ExolvraTestApp;

internal static class Program
{
    private const int DefaultLength = 16;
    private const int MinLength = 4;
    private const int MaxLength = 1024;

    private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Digits = "0123456789";
    private const string Symbols = "!@#$%^&*()-_=+[]{};:,.<>/?";
    private const string Ambiguous = "0O1lI|`'\"";

    private sealed class Options
    {
        public int Length { get; set; } = DefaultLength;
        public int Count { get; set; } = 1;
        public bool IncludeLower { get; set; } = true;
        public bool IncludeUpper { get; set; } = true;
        public bool IncludeDigits { get; set; } = true;
        public bool IncludeSymbols { get; set; } = false;
        public bool ExcludeAmbiguous { get; set; } = false;
        public bool ShowHelp { get; set; } = false;
    }

    public static int Main(string[] args)
    {
        Options opts;
        try
        {
            opts = ParseArgs(args);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            Console.Error.WriteLine("try --help for usage");
            return 1;
        }

        if (opts.ShowHelp)
        {
            PrintHelp();
            return 0;
        }

        if (Console.IsInputRedirected)
        {
            string? line = Console.In.ReadLine();
            if (!string.IsNullOrWhiteSpace(line) && int.TryParse(line.Trim(), out int stdinLen))
            {
                opts.Length = stdinLen;
            }
        }

        if (opts.Length < MinLength || opts.Length > MaxLength)
        {
            Console.Error.WriteLine($"error: length must be between {MinLength} and {MaxLength} (got {opts.Length})");
            return 1;
        }

        if (opts.Count < 1 || opts.Count > 10_000)
        {
            Console.Error.WriteLine($"error: count must be between 1 and 10000 (got {opts.Count})");
            return 1;
        }

        string charset = BuildCharset(opts);
        if (charset.Length == 0)
        {
            Console.Error.WriteLine("error: no character classes enabled — password cannot be generated");
            return 2;
        }

        for (int i = 0; i < opts.Count; i++)
        {
            Console.WriteLine(GeneratePassword(opts.Length, charset));
        }

        return 0;
    }

    private static Options ParseArgs(string[] args)
    {
        var o = new Options();
        for (int i = 0; i < args.Length; i++)
        {
            string a = args[i];
            switch (a)
            {
                case "-h":
                case "--help":
                    o.ShowHelp = true;
                    return o;

                case "-l":
                case "--length":
                    o.Length = RequireInt(args, ref i, a);
                    break;

                case "-n":
                case "--count":
                    o.Count = RequireInt(args, ref i, a);
                    break;

                case "--no-lower":
                    o.IncludeLower = false;
                    break;

                case "--no-upper":
                    o.IncludeUpper = false;
                    break;

                case "--no-digits":
                    o.IncludeDigits = false;
                    break;

                case "-s":
                case "--symbols":
                    o.IncludeSymbols = true;
                    break;

                case "-x":
                case "--exclude-ambiguous":
                    o.ExcludeAmbiguous = true;
                    break;

                default:
                    throw new ArgumentException($"unknown option '{a}'");
            }
        }
        return o;
    }

    private static int RequireInt(string[] args, ref int i, string flag)
    {
        if (i + 1 >= args.Length)
        {
            throw new ArgumentException($"{flag} requires a numeric value");
        }
        string raw = args[++i];
        if (!int.TryParse(raw, out int value))
        {
            throw new ArgumentException($"{flag} expected a number, got '{raw}'");
        }
        return value;
    }

    private static string BuildCharset(Options o)
    {
        var sb = new StringBuilder();
        if (o.IncludeLower) sb.Append(Lowercase);
        if (o.IncludeUpper) sb.Append(Uppercase);
        if (o.IncludeDigits) sb.Append(Digits);
        if (o.IncludeSymbols) sb.Append(Symbols);

        if (!o.ExcludeAmbiguous) return sb.ToString();

        var filtered = new StringBuilder(sb.Length);
        foreach (char c in sb.ToString())
        {
            if (Ambiguous.IndexOf(c) < 0) filtered.Append(c);
        }
        return filtered.ToString();
    }

    private static string GeneratePassword(int length, string charset)
    {
        var chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            int idx = RandomNumberGenerator.GetInt32(charset.Length);
            chars[i] = charset[idx];
        }
        return new string(chars);
    }

    private static void PrintHelp()
    {
        Console.WriteLine("ExolvraTestApp — cryptographic password generator");
        Console.WriteLine();
        Console.WriteLine("Usage: ExolvraTestApp [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -l, --length N           Password length (default: 16, min: 4, max: 1024)");
        Console.WriteLine("  -n, --count N            Number of passwords to generate (default: 1)");
        Console.WriteLine("      --no-lower           Exclude lowercase letters");
        Console.WriteLine("      --no-upper           Exclude uppercase letters");
        Console.WriteLine("      --no-digits          Exclude digits");
        Console.WriteLine("  -s, --symbols            Include symbols (!@#$%^&*...)");
        Console.WriteLine("  -x, --exclude-ambiguous  Exclude visually ambiguous chars (0,O,1,l,I,|)");
        Console.WriteLine("  -h, --help               Show this help");
        Console.WriteLine();
        Console.WriteLine("If stdin is piped and contains a number, it is used as the length.");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  ExolvraTestApp                       # one 16-char password");
        Console.WriteLine("  ExolvraTestApp -l 32 -s              # 32 chars with symbols");
        Console.WriteLine("  ExolvraTestApp -n 5 -x               # 5 unambiguous passwords");
        Console.WriteLine("  echo 24 | ExolvraTestApp             # length from stdin");
    }
}
