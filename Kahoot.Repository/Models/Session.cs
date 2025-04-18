using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class Session
{
    public int SessionId { get; set; }

    public int QuizId { get; set; }

    public string SessionName { get; set; } = null!;

    public string SessionCode { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? EndAt { get; set; }

    public bool EndedManually { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
}
