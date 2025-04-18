using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.ModelDTOs.Response
{
    public class AnswerResponse
    {
        public int AnswerId { get; set; }
        public string AnswerText { get; set; } = null!;
        public bool IsCorrect { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
