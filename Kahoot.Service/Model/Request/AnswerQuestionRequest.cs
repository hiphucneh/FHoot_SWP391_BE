using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Request
{
    public class AnswerQuestionRequest
    {
        public string SessionCode { get; set; }
        public int QuestionSessionId { get; set; }
        public int AnswerId { get; set; }
    }
}
