# Task: Add --show-entropy to print each password's strength in bits

Source issue: dharrisHub/ExolvraTestApp#32

Add a `--show-entropy` flag that reports the entropy of each generated password so users can judge strength.

The generator already exposes everything needed: `PasswordGenerator.AlphabetSize` (`PasswordGenerator.cs:38`). Entropy in bits is `length * log2(alphabetSize)`.

**Proposed behavior**
- With `--show-entropy`, print the bit-strength alongside (or just before) the passwords, e.g. `# entropy: 131.0 bits (alphabet 94, length 20)`.
- Route the annotation to stderr, or suppress it under `-q/--quiet` (`Program.cs:46`) so piped output stays clean.
- A 16-char password over the default 62-char alphabet should report ~95.3 bits.

**Acceptance**
- `ExolvraTestApp -l 20 -s --show-entropy` prints a plausible bit value plus the password.
- `-q` suppresses the entropy line; the password still prints.
