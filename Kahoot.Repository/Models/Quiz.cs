using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class Quiz
{
    public int QuizId { get; set; }

    public string QuizName { get; set; } = null!;

    public string? Description { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
