using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class QuizSession
{
    public int QuizSessionId { get; set; }

    public int QuizId { get; set; }

    public string SessionName { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool? EndedManually { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<QuizSessionTeam> QuizSessionTeams { get; set; } = new List<QuizSessionTeam>();

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
