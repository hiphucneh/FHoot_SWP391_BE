using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Response
{
    public class UserSessionInfoResponse
    {
        public string SessionName { get; set; }
        public int SessionId { get; set; }
        public string SessionCode { get; set; }
        public DateTime? EndAt { get; set; }
    }

}
