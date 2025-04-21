using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Request
{
    public class TeamRequest
    {
        public string SessionCode { get; set; }

        public string TeamName { get; set; } = null!;
    }
}
