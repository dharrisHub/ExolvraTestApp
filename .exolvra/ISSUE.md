# Task: Accept --flag=value syntax (e.g. --length=32)

Source issue: dharrisHub/ExolvraTestApp#33

Today flags only accept a space-separated value: `--length 32`. `ParseArgs` (`Program.cs:149-235`) reads the value as the next argument via `RequireInt`/`RequireValue` (`Program.cs:237-258`), so `--length=32` falls through to the `default` case and errors with `unknown option '--length=32'`.

**Ask:** also accept `--flag=value` for the value-taking options (`--length`, `--count`, `--min-digits`, `--max-length`, `--exclude-chars`). Split on the first `=` before the switch, keeping the existing space-separated form working.

**Acceptance**
- `ExolvraTestApp --length=12` and `ExolvraTestApp -l 12` behave identically.
- `--exclude-chars=abc` works.
- Unknown `--foo=bar` still errors clearly.
- A unit test covers the `=` form.
