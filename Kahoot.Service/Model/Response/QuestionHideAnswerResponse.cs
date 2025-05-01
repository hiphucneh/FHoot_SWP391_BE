using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kahoot.Service.ModelDTOs.Response;

namespace Kahoot.Service.Model.Response
{
    public class QuestionHideAnswerResponse
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = null!;
        public int SortOrder { get; set; }
        public bool IsRandomAnswer { get; set; }
        public string? ImgUrl { get; set; }
        public int TimeLimitSec { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<SessionAnswerResponse> Answers { get; set; } = new();
    }
}
