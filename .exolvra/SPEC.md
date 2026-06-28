# Spec: Test coverage for `-x/--exclude-ambiguous` and `--exclude-chars`

Source issue: dharrisHub/ExolvraTestApp#34

## Goal
Add automated test coverage for the two character-filter options that currently have none:
- `-x` / `--exclude-ambiguous` — removes `AmbiguousChars` (`0O1lI|` + backtick + `'` + `"`, defined at `PasswordGenerator.cs:16`).
- `--exclude-chars <set>` — removes each listed character.

Tests must also confirm the filters apply to the **digit alphabet** used by `--min-digits`
(`BuildDigitAlphabet`, `PasswordGenerator.cs:113-118`).

## Background (verified in code)
- `Program.Main` maps `-x/--exclude-ambiguous` → `ExcludeAmbiguous` and `--exclude-chars` → `ExcludedChars`
  (`Program.cs:204-211`).
- `ApplyFilters` (`PasswordGenerator.cs:129-143`) is applied to the full alphabet, the digit alphabet, and
  the letter alphabet, so excluded chars never appear in any password position, including the digit
  positions forced by `--min-digits`.
- An empty resulting alphabet exits with code 2 and `error: resulting alphabet is empty …`
  (`Program.cs:123-127`). An empty digit alphabet with `--min-digits > 0` exits 2 with
  `error: min-digits requires at least one digit …` (`Program.cs:129-133`).
- `AmbiguousChars` contains digits `0` and `1`; the other ambiguous chars (`O l I |` backtick `' "`) are not
  in the default alphabet (symbols are off by default), so a robust ambiguous test should either enable
  symbols or simply assert that none of the full `AmbiguousChars` set appears.

## Approach
Add two new test files following the existing one-option-per-file pattern (e.g. `NoSimilarOptionTests.cs`).
Each file is self-contained: same `RunCli` helper and private `CliRun` record used by the sibling test
files, driving `Program.Main` with captured stdout/stderr.

### 1. `tests/ExolvraTestApp.Tests/ExcludeAmbiguousOptionTests.cs`
- **Main_WhenExcludeAmbiguousIsPassed_OmitsAllAmbiguousCharacters** — run with `--quiet --count 20
  --length 64 --symbols -x` (symbols enabled so ambiguous symbol chars like `|` backtick `'` `"` are
  actually candidates). Assert exit 0, empty stderr, 20 passwords of length 64, and that no character in
  `PasswordGenerator.AmbiguousChars` appears in any password.
- **Main_WhenExcludeAmbiguousAndMinDigits_OmitsAmbiguousDigits** — run with `--quiet --count 20 --length 12
  --min-digits 6 -x`. Assert exit 0, ≥6 digits per password, and that `0` and `1` (the ambiguous digits)
  never appear — proving the filter reaches the digit alphabet.
- Use the short alias `-x` in one test and `--exclude-ambiguous` in another to cover both spellings.

### 2. `tests/ExolvraTestApp.Tests/ExcludeCharsOptionTests.cs`
- **Main_WhenExcludeCharsIsPassed_OmitsListedCharacters** — run with `--quiet --count 20 --length 64
  --symbols --exclude-chars "abc!@#"` (sample). Assert exit 0, empty stderr, 20 passwords of length 64, and
  none of the listed chars appear.
- **Main_WhenExcludeCharsRemovesDigits_AppliesToMinDigitAlphabet** — run with `--quiet --count 20
  --length 12 --min-digits 6 --exclude-chars "02468"`. Assert exit 0, ≥6 digits per password, and that
  none of `0 2 4 6 8` appear — proving the filter reaches the digit alphabet.
- **Main_WhenExcludeCharsEmptiesAlphabet_ReturnsEmptyAlphabetError** — run with `--no-upper --no-digits
  --exclude-chars "abcdefghijklmnopqrstuvwxyz"`. Assert exit 2, empty stdout, and stderr contains
  `resulting alphabet is empty`.

## Files to change
- **Add** `tests/ExolvraTestApp.Tests/ExcludeAmbiguousOptionTests.cs`
- **Add** `tests/ExolvraTestApp.Tests/ExcludeCharsOptionTests.cs`

No production code changes.

## Verification
- `dotnet test` passes (all existing + new tests).

## Risks / open questions
- Minimal risk: tests only. Large samples (20 × 64 chars) make the probabilistic "char never appears"
  assertions reliable while staying fast.
- Open question: the existing files do **not** consolidate the `RunCli` helper into a shared base — each
  duplicates it. I will follow that established convention rather than refactoring to a shared helper, to
  keep this change focused on coverage. Flag if you'd prefer a shared test helper instead.
