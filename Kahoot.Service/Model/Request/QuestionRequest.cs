using Kahoot.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.ModelDTOs.Request
{
    public class QuestionRequest
    {
        public string QuestionText { get; set; }
        public int TimeLimitSec { get; set; }
        public bool IsRandomAnswer { get; set; }
        public ICollection<AnswerRequest> Answers { get; set; } = new List<AnswerRequest>();
    }
}

