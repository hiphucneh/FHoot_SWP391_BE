using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Response
{
    public class AnswerTotalScoreResponse
    {
        public bool IsCorrect { get; set; }
        public int Score { get; set; }
        public int AnswerOrder { get; set; }
        public int TotalScore { get; set; }
    }
}
