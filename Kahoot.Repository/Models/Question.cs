using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public int QuizId { get; set; }

    public string QuestionText { get; set; } = null!;

    public int SortOrder { get; set; }

    public int TimeLimitSec { get; set; }

    public virtual Answer? Answer { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
