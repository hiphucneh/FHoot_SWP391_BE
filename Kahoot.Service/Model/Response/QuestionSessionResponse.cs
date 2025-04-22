using Kahoot.Service.ModelDTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Response
{
    public class QuestionSessionResponse
    {
        public int QuestionSessionId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = null!;
        public int SortOrder { get; set; }
        public int TimeLimitSec { get; set; }
        public DateTime RunAt { get; set; }
        public QuestionResponse Question { get; set; } = null!;
    }
}
