using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Request
{
    public class CreateSessionRequest
    {
        /// <summary>
        /// ID của Quiz được sử dụng cho phiên chơi
        /// </summary>
        public int QuizId { get; set; }

        /// <summary>
        /// Tên hiển thị của phiên chơi
        /// </summary>
        public string SessionName { get; set; }
    }
}
