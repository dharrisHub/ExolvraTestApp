using System;

namespace ExolvraTestApp;

internal static class Program
{
    private const int DefaultLength = 16;
    private const int MinCount = 1;
    private const int MaxCount = 10_000;

    private sealed class Options
    {
        public int Length { get; set; } = DefaultLength;
        public int Count { get; set; } = 1;
        public bool IncludeLower { get; set; } = true;
        public bool IncludeUpper { get; set; } = true;
        public bool IncludeDigits { get; set; } = true;
        public bool IncludeSymbols { get; set; }
        public bool ExcludeAmbiguous { get; set; }
        public bool ShowHelp { get; set; }
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

        if (opts.Length < PasswordGenerator.MinLength || opts.Length > PasswordGenerator.MaxLength)
        {
            Console.Error.WriteLine(
                $"error: length must be between {PasswordGenerator.MinLength} and {PasswordGenerator.MaxLength} (got {opts.Length})");
            return 1;
        }

        if (opts.Count < MinCount || opts.Count > MaxCount)
        {
            Console.Error.WriteLine($"error: count must be between {MinCount} and {MaxCount} (got {opts.Count})");
            return 1;
        }

        var generator = new PasswordGenerator(new PasswordGenerator.Charset(
            IncludeLower: opts.IncludeLower,
            IncludeUpper: opts.IncludeUpper,
            IncludeDigits: opts.IncludeDigits,
            IncludeSymbols: opts.IncludeSymbols,
            ExcludeAmbiguous: opts.ExcludeAmbiguous));

        if (generator.AlphabetSize == 0)
        {
            Console.Error.WriteLine("error: no character classes enabled — password cannot be generated");
            return 2;
        }

        for (int i = 0; i < opts.Count; i++)
        {
            Console.WriteLine(generator.Generate(opts.Length));
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
