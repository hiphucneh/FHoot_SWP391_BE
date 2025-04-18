using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class Team
{
    public int TeamId { get; set; }

    public int SessionId { get; set; }

    public string TeamName { get; set; } = null!;

    public int TotalScore { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();

    public virtual Session Session { get; set; } = null!;
}
