using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.ModelDTOs.Response
{
    public class QuizResponse
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImgUrl { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public List<QuestionResponse> Questions { get; set; } = new();
    }
}
