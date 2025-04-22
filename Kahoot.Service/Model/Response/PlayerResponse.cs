using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Response
{
    public class PlayerResponse
    {
        public int PlayerId { get; set; }
        public int TeamId { get; set; }
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public int Score { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
