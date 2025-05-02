using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Response
{
    public class PlayerSessionResultResponse
    {
        public int PlayerId { get; set; }
        public int TotalScore { get; set; }
        public List<PlayerAnswerInfo> Answers { get; set; }
    }

    public class PlayerAnswerInfo
    {
        public int QuestionSessionId { get; set; }
        public int AnswerId { get; set; }
        public bool IsCorrect { get; set; }
        public int Score { get; set; }
        public int AnswerOrder { get; set; }
        public string QuestionText { get; set; } 
        public string AnswerText { get; set; }  
    }

}
