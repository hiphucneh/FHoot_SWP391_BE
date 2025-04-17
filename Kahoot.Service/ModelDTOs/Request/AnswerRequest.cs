using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.ModelDTOs.Request
{
    public class AnswerRequest
    {
        public string AnswerText { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}
