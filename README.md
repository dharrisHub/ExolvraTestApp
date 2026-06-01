# ExolvraTestApp

A small, no-dependency **cryptographic password generator** for the command line, written in C# on .NET 10. It uses `System.Security.Cryptography.RandomNumberGenerator.GetInt32` for unbiased random character selection — not `System.Random` or `Math.Random` — so the output is suitable for real secrets, not just toy passwords. Character classes, length, and count are all controlled via CLI flags; length can also be piped in on stdin for shell-pipeline use.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) to build.
- .NET 10 runtime to run the built binary. (Newer runtimes work too — the project is published with `DOTNET_ROLL_FORWARD=LatestMajor` friendly defaults.)

## Getting it

```bash
git clone https://github.com/dharrisHub/ExolvraTestApp.git
cd ExolvraTestApp
```

## Build & run

Build a Release binary:

```bash
dotnet build -c Release
```

Run via the SDK (args after `--` go to the app, not `dotnet run`):

```bash
dotnet run -- -l 20 -s
```

Or invoke the built binary directly:

```bash
# Windows
bin\Release\net10.0\ExolvraTestApp.exe -l 20 -s

# Linux / macOS
dotnet bin/Release/net10.0/ExolvraTestApp.dll -l 20 -s
```

## Usage

```
ExolvraTestApp [options]
```

| Flag | Long form | Default | Effect |
|---|---|---|---|
| `-l N` | `--length N` | `16` | Password length. Min `4`, max `1024`. |
| `-n N` | `--count N`, `--num N` | `1` | Number of passwords to generate (one per line). Max `10000`. |
|  | `--no-lower` | off | Exclude lowercase letters `a–z`. |
|  | `--no-upper` | off | Exclude uppercase letters `A–Z`. |
|  | `--no-digits` | off | Exclude digits `0–9`. |
| `-s` | `--symbols` | off | Include symbols `!@#$%^&*()-_=+[]{};:,.<>/?`. |
|  | `--no-symbols` | off | Exclude symbols, even when `--symbols` is also passed. |
| `-x` | `--exclude-ambiguous` | off | Strip visually ambiguous characters: `0 O 1 l I \| ` `` ` `` `'` `"`. |
|  | `--exclude-chars CHARS` | empty | Remove every listed character from the generated alphabet after class flags and ambiguous-character filtering. |
|  | `--min-digits N` | `0` | Require at least `N` digit characters in every generated password. |
|  | `--max-length N` | none | Cap the final generated password length at `N`. |
| `-q` | `--quiet` | off | Suppress non-password output. Generated passwords are still printed one per line. |
| `-v` | `--version` |  | Print the application version and exit without generating a password. |
| `-h` | `--help` |  | Print help and exit. |

If **stdin is piped** and its first line is a number, that number is used as the password length (overriding `-l`).

### Examples

One 16-char password (default):

```bash
$ ExolvraTestApp
wXmYAIoMcpMxys3K
```

32 chars including symbols:

```bash
$ ExolvraTestApp -l 32 -s
R9EX2b>q]Mv$hf,^4gA11c$(edTq[v9w
```

Five passwords, no visually ambiguous chars:

```bash
$ ExolvraTestApp --num 5 -x
```

Exclude specific characters from the generated alphabet:

```bash
$ ExolvraTestApp -l 20 --exclude-chars O0l1
```

Letters and digits only, even if a script also passes `--symbols`:

```bash
$ ExolvraTestApp --symbols --no-symbols
```

Quiet mode prints only generated password lines:

```bash
$ ExolvraTestApp --quiet --num 2
```

Require at least four digits in every generated password:

```bash
$ ExolvraTestApp --length 16 --min-digits 4
```

Cap a requested length at 20 characters:

```bash
$ ExolvraTestApp --length 32 --max-length 20
```

Print the application version:

```bash
$ ExolvraTestApp --version
ExolvraTestApp 1.0.0
```

Digits only, length 8 (e.g. a PIN):

```bash
$ ExolvraTestApp -l 8 --no-lower --no-upper
```

Length from stdin (useful in shell pipelines):

```bash
$ echo 24 | ExolvraTestApp
```

## Exit codes

| Code | Meaning | Example trigger |
|---|---|---|
| `0` | Success — password(s) printed to stdout. | `ExolvraTestApp --help` or any valid generation. |
| `1` | Invalid argument — bad flag, non-numeric value, or length/count/min-digits/max-length out of range. | `-l 2` → `error: length must be between 4 and 1024 (got 2)`. `--max-length 2` → `error: max-length must be between 4 and 1024 (got 2)`. `--min-digits 20 --length 16` → `error: min-digits must be between 0 and the password length 16 (got 20)`. |
| `2` | Impossible configuration — resulting alphabet is empty or cannot satisfy `--min-digits`. | `--no-lower --no-upper --no-digits` (without `-s`) or `--no-digits --min-digits 1` → `error: min-digits requires at least one digit in the generated alphabet`. |

Errors go to **stderr**; passwords go to **stdout**, one per line, so output is pipe-friendly (`ExolvraTestApp -n 10 \| head -1`, etc.).

## Security notes

- Randomness comes from the OS CSPRNG via `RandomNumberGenerator.GetInt32`, which rejects modulo bias — each character in the enabled charset is equally likely.
- No password is ever written to a file or logged; it is only printed to stdout. Where you redirect that stream is up to you.
- `--min-digits N` satisfies digit-minimum policies by drawing at least `N` characters from the filtered digit alphabet, then shuffling them into the password with the same CSPRNG used for normal generation.
