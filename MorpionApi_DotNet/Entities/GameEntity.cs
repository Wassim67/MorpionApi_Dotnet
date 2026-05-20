using Microsoft.AspNetCore.Identity;

namespace MorpionApi_DotNet.Entities;

public class GameEntity
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public required string Board { get; set; }
    public bool IsGameOver { get; set; }
    public int MovesCount { get; set; }
    public required string StatusMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public IdentityUser? User { get; set; }
}
