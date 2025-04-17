using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class Team
{
    public int TeamId { get; set; }

    public int SessionId { get; set; }

    public string TeamName { get; set; } = null!;

    public int? TotalScore { get; set; }

    public virtual Session Session { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
