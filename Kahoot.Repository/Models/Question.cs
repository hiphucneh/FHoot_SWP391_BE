using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public int QuizId { get; set; }

    public string QuestionText { get; set; } = null!;

    public bool IsRandomAnswer { get; set; }

    public string? ImgUrl { get; set; }

    public int SortOrder { get; set; }

    public int TimeLimitSec { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<QuestionSection> QuestionSections { get; set; } = new List<QuestionSection>();

    public virtual Quiz Quiz { get; set; } = null!;
}
