using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class PlayerAnswer
{
    public int PlayerAnswerId { get; set; }

    public int PlayerId { get; set; }

    public int QuestionSessionId { get; set; }

    public int AnswerId { get; set; }

    public DateTime AnswerTime { get; set; }

    public bool IsCorrect { get; set; }

    public int Score { get; set; }

    public virtual Answer Answer { get; set; } = null!;

    public virtual Player Player { get; set; } = null!;

    public virtual QuestionSession QuestionSession { get; set; } = null!;
}
