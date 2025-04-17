using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class UserAnswer
{
    public int UserAnswerId { get; set; }

    public int SessionId { get; set; }

    public int UserId { get; set; }

    public int QuestionId { get; set; }

    public int AnswerId { get; set; }

    public int AnswerOrder { get; set; }

    public bool IsCorrect { get; set; }

    public int Score { get; set; }

    public DateTime? AnswerTime { get; set; }

    public virtual Answer Answer { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;

    public virtual Session Session { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
