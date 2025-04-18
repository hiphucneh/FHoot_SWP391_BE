using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class Section
{
    public int SectionId { get; set; }

    public int SessionId { get; set; }

    public string SectionName { get; set; } = null!;

    public DateTime StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public virtual ICollection<QuestionSection> QuestionSections { get; set; } = new List<QuestionSection>();

    public virtual Session Session { get; set; } = null!;
}
