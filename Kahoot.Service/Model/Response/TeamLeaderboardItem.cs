using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Response
{
    public class TeamLeaderboardItem
    {
        public int Rank { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; } = null!;
        public int TotalScore { get; set; }
    }
}
