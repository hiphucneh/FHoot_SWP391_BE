using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class PlayerResponse
{
    public int ResponseId { get; set; }

    public int SessionUserId { get; set; }

    public int QuestionId { get; set; }

    public int? AnswerId { get; set; }

    public DateTime? AnsweredAt { get; set; }

    public int? PointsAwarded { get; set; }

    public virtual Answer? Answer { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual GameSessionUser SessionUser { get; set; } = null!;
}
