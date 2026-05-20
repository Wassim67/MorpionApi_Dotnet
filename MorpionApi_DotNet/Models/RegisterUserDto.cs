namespace MorpionApi_DotNet.Models;

public class RegisterUserDto
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
