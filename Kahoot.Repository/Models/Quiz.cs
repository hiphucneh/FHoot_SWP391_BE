using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class Quiz
{
    public int QuizId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updateat { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
