﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Response
{
    public class JoinSessionResponse
    {
        public int SessionId { get; set; }
        public string SessionCode { get; set; }
        public int QuizId { get; set; }
        public string SessionName { get; set; }
        public DateTime StartedAt { get; set; }
    }
}
