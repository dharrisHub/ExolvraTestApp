namespace ExolvraTestApp;

public sealed record PasswordCliRequest(int Length, int Count, bool Quiet);

public sealed class PasswordCliService
{
    private readonly IPasswordGenerator _generator;

    public PasswordCliService(IPasswordGenerator generator)
    {
        _generator = generator;
    }

    public void WritePasswords(PasswordCliRequest request, TextWriter output)
    {
        // Generated output stays password-only so --quiet remains safe for scripts.
        for (int i = 0; i < request.Count; i++)
        {
            output.WriteLine(_generator.Generate(request.Length));
        }
    }
}
