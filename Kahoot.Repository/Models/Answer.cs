using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class Answer
{
    public int AnswerId { get; set; }

    public int QuestionId { get; set; }

    public string AnswerText { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public virtual ICollection<PlayerResponse> PlayerResponses { get; set; } = new List<PlayerResponse>();

    public virtual Question Question { get; set; } = null!;
}
