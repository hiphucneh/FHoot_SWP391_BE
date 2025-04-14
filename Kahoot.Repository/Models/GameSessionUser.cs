using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class GameSessionUser
{
    public int SessionUserId { get; set; }

    public int SessionId { get; set; }

    public int UserId { get; set; }

    public DateTime? JoinedAt { get; set; }

    public int? Score { get; set; }

    public virtual ICollection<PlayerResponse> PlayerResponses { get; set; } = new List<PlayerResponse>();

    public virtual GameSession Session { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
