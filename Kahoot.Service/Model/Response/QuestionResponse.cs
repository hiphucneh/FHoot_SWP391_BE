using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.ModelDTOs.Response
{
    public class QuestionResponse
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = null!;
        public int SortOrder { get; set; }
        public bool IsRandomAnswer { get; set; }
        public string? ImgUrl { get; set; }
        public int TimeLimitSec { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<AnswerResponse> Answers { get; set; } = new();
    }
}
