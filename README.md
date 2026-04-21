# ExolvraTestApp

A small, no-dependency **cryptographic password generator** for the command line, written in C# on .NET 8. It uses `System.Security.Cryptography.RandomNumberGenerator.GetInt32` for unbiased random character selection — not `System.Random` or `Math.Random` — so the output is suitable for real secrets, not just toy passwords. Character classes, length, and count are all controlled via CLI flags; length can also be piped in on stdin for shell-pipeline use.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (LTS) to build.
- .NET 8 runtime to run the built binary. (Newer runtimes work too — the project is published with `DOTNET_ROLL_FORWARD=LatestMajor` friendly defaults.)

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
bin\Release\net8.0\ExolvraTestApp.exe -l 20 -s

# Linux / macOS
dotnet bin/Release/net8.0/ExolvraTestApp.dll -l 20 -s
```

## Usage

```
ExolvraTestApp [options]
```

| Flag | Long form | Default | Effect |
|---|---|---|---|
| `-l N` | `--length N` | `16` | Password length. Min `4`, max `1024`. |
| `-n N` | `--count N` | `1` | Number of passwords to generate (one per line). Max `10000`. |
|  | `--no-lower` | off | Exclude lowercase letters `a–z`. |
|  | `--no-upper` | off | Exclude uppercase letters `A–Z`. |
|  | `--no-digits` | off | Exclude digits `0–9`. |
| `-s` | `--symbols` | off | Include symbols `!@#$%^&*()-_=+[]{};:,.<>/?`. |
| `-x` | `--exclude-ambiguous` | off | Strip visually ambiguous characters: `0 O 1 l I \| ` `` ` `` `'` `"`. |
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
$ ExolvraTestApp -n 5 -x
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
| `1` | Invalid argument — bad flag, non-numeric value, or length/count out of range. | `-l 2` → `error: length must be between 4 and 1024 (got 2)`. `--wat` → `error: unknown option '--wat'`. |
| `2` | Impossible configuration — every character class disabled, charset empty. | `--no-lower --no-upper --no-digits` (without `-s`) → `error: no character classes enabled`. |

Errors go to **stderr**; passwords go to **stdout**, one per line, so output is pipe-friendly (`ExolvraTestApp -n 10 \| head -1`, etc.).

## Security notes

- Randomness comes from the OS CSPRNG via `RandomNumberGenerator.GetInt32`, which rejects modulo bias — each character in the enabled charset is equally likely.
- No password is ever written to a file or logged; it is only printed to stdout. Where you redirect that stream is up to you.
- The app does **not** enforce character-class minimums (e.g. "at least one digit"). For a 16-char password over `[a–zA–Z0–9]` the probability of missing any one class is negligible, and enforcing minimums reduces entropy. If a policy requires it, regenerate or filter downstream.
