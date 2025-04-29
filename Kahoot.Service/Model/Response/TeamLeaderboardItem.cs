using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Response
{
    public class TeamLeaderboardItem
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int TotalScore { get; set; }
        public int Rank { get; set; }
        public List<PlayerLeaderboardItem> Players { get; set; }
    }

    public class PlayerLeaderboardItem
    {
        public int PlayerId { get; set; }
        public string Name { get; set; }
        public int TotalScore { get; set; }
    }

}
