using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class GameSession
{
    public int SessionId { get; set; }

    public int QuizId { get; set; }

    public int HostUserId { get; set; }

    public string SessionCode { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<GameSessionUser> GameSessionUsers { get; set; } = new List<GameSessionUser>();

    public virtual User HostUser { get; set; } = null!;

    public virtual Quiz Quiz { get; set; } = null!;
}
