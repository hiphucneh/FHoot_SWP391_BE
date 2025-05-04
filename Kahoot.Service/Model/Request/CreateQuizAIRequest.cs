using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kahoot.Common.Enums;

namespace Kahoot.Service.Model.Request
{
    using System.ComponentModel.DataAnnotations;

    public class CreateQuizAIRequest
    {
        public string Topic { get; set; }

        public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Medium;

        public int NumberOfQuestions { get; set; } = 5;

        [Range(2, 4, ErrorMessage = "Số lượng câu trả lời phải nằm trong khoảng từ 2 đến 4.")]
        public int NumberOfAnswers { get; set; } = 4;
    }


}
