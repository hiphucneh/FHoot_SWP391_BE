using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? Age { get; set; }

    public string? Avatar { get; set; }

    public string? FcmToken { get; set; }

    public string? Status { get; set; }

    public int RoleId { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    public virtual ICollection<QuizSessionTeam> QuizSessionTeams { get; set; } = new List<QuizSessionTeam>();
}
