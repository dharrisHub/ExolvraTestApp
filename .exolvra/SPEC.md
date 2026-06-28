# Spec: Accept `--flag=value` syntax

Source issue: dharrisHub/ExolvraTestApp#33

## Goal
Let the value-taking long options also accept an inline `--flag=value` form, in addition to
the existing space-separated `--flag value` form. `--length=32` should behave exactly like
`-l 32` / `--length 32`.

## Current behavior
`ParseArgs` (`Program.cs:96-155`) switches on the whole argument token. A value is read from the
*next* argument via `RequireInt` / `RequireValue` (`Program.cs:157-178`). So `--length=32` is one
token that matches no `case` and hits `default`, erroring with `unknown option '--length=32'`.

## Scope note (discrepancy with issue text)
The issue lists `--min-digits` and `--max-length` as value-taking options. **Those options do not
exist in this codebase.** The real value-taking options are:

- `-l` / `--length`
- `-n` / `--count`
- `--exclude-chars`

This spec covers exactly those three. (`--min-digits` / `--max-length` are out of scope —
nothing to wire them to.)

## Approach
In `ParseArgs`, before the `switch`, detect an inline value:

- If the token starts with `--` and contains `=`, split on the **first** `=` into `name` + inline
  `value`, but **only when `name` is one of the value-taking options** (`--length`, `--count`,
  `--exclude-chars`). Splitting only for known value-takers means:
  - `--exclude-chars=a=b=c` → name `--exclude-chars`, value `a=b=c` (first `=` only).
  - `--symbols=true` (a boolean flag) is **not** split, so it falls through to `default` and errors
    clearly as `unknown option '--symbols=true'` rather than silently ignoring the `=true`.
  - `--foo=bar` is not a known value-taker → `default` → `unknown option '--foo=bar'` (full token
    preserved in the message).

`RequireInt` / `RequireValue` gain an optional `inline` parameter:
- When an inline value is present, use it directly (do **not** consume the next argument).
- When absent, behavior is unchanged (consume next argument; same error messages).

Empty inline values are passed through as-is: `--exclude-chars=` yields an empty exclude set
(harmless), and `--length=` fails `int.TryParse` → `--length expected a number, got ''` (clear).

Short forms keep their existing space-separated behavior only (e.g. `-l 32`). `-l=32` is **not**
introduced — the issue asks specifically for `--flag=value`, and `-l=32` would parse `=32` as the
value. Keeping short options as-is avoids that footgun.

## Files to change
- `Program.cs`
  - Add inline-value detection ahead of the `switch` in `ParseArgs`.
  - Thread an `inline` argument through `RequireInt` / `RequireValue`.
  - Make `ParseArgs` and the `Options` type `internal` (currently `private`) and add
    `[assembly: InternalsVisibleTo("ExolvraTestApp.Tests")]` so the parser can be unit-tested.
- `ExolvraTestApp.csproj`
  - No functional change required; the `InternalsVisibleTo` is declared via an attribute in code.

## New test project (required by acceptance: "A unit test covers the `=` form")
There is no test project today. Add `tests/ExolvraTestApp.Tests/`:
- `ExolvraTestApp.Tests.csproj` — xUnit, `net10.0`, `ProjectReference` to `ExolvraTestApp.csproj`.
- `ParseArgsTests.cs` covering:
  - `--length=32` parses Length 32; equals the result of parsing `-l 32` and `--length 32`.
  - `--count=5` parses Count 5.
  - `--exclude-chars=abc` sets ExcludedChars `abc`; `--exclude-chars=a=b` → `a=b`.
  - `--length=` and `--length=x` throw `ArgumentException`.
  - `--foo=bar` throws `ArgumentException` (unknown option).
  - Existing space-separated forms still parse correctly (regression guard).

## CI
Add a `dotnet test` step to `.github/workflows/ci.yml` (after the build step) that runs the new
test project, so the `=`-form test guards regressions in CI.

## Risks
- Low. Change is localized to argument parsing; space-separated path is preserved unchanged.
- Introducing a test project is new infrastructure for this repo (xUnit dependency).

## Decisions (confirmed)
1. Test framework: **xUnit** project at `tests/ExolvraTestApp.Tests/`.
2. CI: **add a `dotnet test` step** to `ci.yml`.
3. Flag scope: **only the existing value-taking options** (`--length`/`-l`, `--count`/`-n`,
   `--exclude-chars`); the nonexistent `--min-digits` / `--max-length` are not implemented.
