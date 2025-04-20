using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Request
{
    public class JoinSessionRequest
    {
        /// <summary>
        /// Mã code của phiên chơi do host cấp
        /// </summary>
        public string SessionCode { get; set; }

        /// <summary>
        /// ID của người chơi
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Tên đầy đủ của người chơi
        /// </summary>
        public string FullName { get; set; }
    }
}
