using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class QuestionSession
{
    public int QuestionSessionId { get; set; }

    public int SessionId { get; set; }

    public int QuestionId { get; set; }

    public int SortOrder { get; set; }

    public DateTime RunAt { get; set; }

    public virtual ICollection<PlayerAnswer> PlayerAnswers { get; set; } = new List<PlayerAnswer>();

    public virtual Question Question { get; set; } = null!;

    public virtual Session Session { get; set; } = null!;
}
