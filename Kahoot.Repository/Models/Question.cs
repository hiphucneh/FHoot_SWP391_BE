using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public int QuizId { get; set; }

    public string Content { get; set; } = null!;

    public string? QuestionType { get; set; }

    public int? TimeLimit { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<PlayerResponse> PlayerResponses { get; set; } = new List<PlayerResponse>();

    public virtual Quiz Quiz { get; set; } = null!;
}
