# Task: Add test coverage for -x/--exclude-ambiguous and --exclude-chars

Source issue: dharrisHub/ExolvraTestApp#34

The test suite covers max-length, min-digits, no-similar, no-symbols, no-uppercase, and start-with-letter (see `tests/ExolvraTestApp.Tests/`), but there are no tests for the ambiguous/excluded-character filters.

Add tests asserting:
- With `-x/--exclude-ambiguous`, none of `AmbiguousChars` (`0O1lI|`, backtick, `'`, `"` â€” defined at `PasswordGenerator.cs:16`) ever appear across a large sample.
- With `--exclude-chars <set>`, none of the listed characters appear.
- The filters also apply to the digit alphabet used by `--min-digits` (`BuildDigitAlphabet`, `PasswordGenerator.cs:113-118`).

**Acceptance**
- New test files follow the existing one-option-per-file pattern in `tests/ExolvraTestApp.Tests/`.
- Tests pass via `dotnet test`.
