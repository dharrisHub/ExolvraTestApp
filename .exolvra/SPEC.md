# Spec: `--show-entropy` flag

Source issue: dharrisHub/ExolvraTestApp#32

## Goal
Add a `--show-entropy` flag that reports the Shannon entropy (in bits) of the
generated passwords so users can judge their strength. Entropy in bits is
`length * log2(alphabetSize)`.

Because every password in a single run shares the same alphabet and length, the
entropy is identical for all of them, so a single annotation line is printed
once per run (matching the example in the issue), not one line per password.

## Proposed behavior
- New flag `--show-entropy` (no short form). Off by default.
- When set, print one annotation line of the form:
  `# entropy: 95.3 bits (alphabet 62, length 16)`
  - Bits formatted to one decimal place using `InvariantCulture` (so the
    decimal separator is always `.` regardless of locale).
  - `alphabet` = `generator.AlphabetSize`, `length` = the effective password
    length (after any `--max-length` cap).
- The line is printed **to stderr**, before the passwords, so default piped
  stdout output stays clean even without `-q`. (See open question.)
- Suppressed when `-q/--quiet` is set. Passwords still print.
- Example math: 16 chars over the default 62-char alphabet →
  `16 * log2(62) = 95.27 → 95.3 bits`. A length-20 password with symbols
  uses this app's 88-char alphabet (62 + 26 symbols, not the issue's generic
  94) → `20 * log2(88) = 129.2 bits`.

## Files to change
- `Program.cs`
  - Add `ShowEntropy` to the `Options` record.
  - Parse `--show-entropy` in `ParseArgs`.
  - After the generator is built and validated (alphabet non-empty), and before
    the generation loop, emit the entropy line when `ShowEntropy` is set and the
    run is not quiet.
  - Add a help-text line for `--show-entropy`.
- `tests/ExolvraTestApp.Tests/ShowEntropyOptionTests.cs` (new)
  - `--show-entropy` prints a plausible bit value (~95.3 for default length 16)
    plus the password(s).
  - `-q` (or `--quiet`) suppresses the entropy line; passwords still print.
  - `--help` mentions `--show-entropy`.

## Acceptance (from issue)
- `ExolvraTestApp -l 20 -s --show-entropy` prints a plausible bit value plus the
  password.
- `-q` suppresses the entropy line; the password still prints.

## Risks / notes
- Entropy is computed from alphabet size and length only — it does not account
  for `--min-digits` or `--start-with-letter` constraints, which slightly reduce
  true entropy. This matches the formula the issue specifies; I will not model
  the constraint reduction unless asked.
- Locale-safe formatting via `InvariantCulture` keeps tests deterministic.

## Routing decision (resolved)
The issue offered two routing options ("route to stderr, OR suppress under -q").
Confirmed with the user: **stderr + suppress-under-`-q`**. The line goes to
stderr before the passwords, and is omitted entirely when `-q/--quiet` is set.
