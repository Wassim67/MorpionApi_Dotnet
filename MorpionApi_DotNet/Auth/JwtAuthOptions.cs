namespace MorpionApi_DotNet.Auth;

public class JwtAuthOptions
{
    public const string SectionName = "JwtAuth";

    public required string Key { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int ExpirationInMinutes { get; init; }
}
