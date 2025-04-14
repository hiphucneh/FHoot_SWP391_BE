using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Phone { get; set; }

    public int? Age { get; set; }

    public string? Gender { get; set; }

    public string? Location { get; set; }

    public string? Avatar { get; set; }

    public string? FcmToken { get; set; }

    public string? Status { get; set; }

    public bool? EnableReminder { get; set; }

    public int RoleId { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public virtual ICollection<GameSessionUser> GameSessionUsers { get; set; } = new List<GameSessionUser>();

    public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<UserPackage> UserPackages { get; set; } = new List<UserPackage>();
}
