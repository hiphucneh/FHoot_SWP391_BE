using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class QuizSessionTeam
{
    public int QuizSessionTeamId { get; set; }

    public int QuizSessionId { get; set; }

    public string TeamName { get; set; } = null!;

    public int? TotalScore { get; set; }

    public virtual QuizSession QuizSession { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
