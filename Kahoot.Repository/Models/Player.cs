using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class Player
{
    public int PlayerId { get; set; }

    public int UserId { get; set; }

    public int TeamId { get; set; }

    public DateTime JoinedAt { get; set; }

    public virtual ICollection<PlayerAnswer> PlayerAnswers { get; set; } = new List<PlayerAnswer>();

    public virtual Team Team { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
