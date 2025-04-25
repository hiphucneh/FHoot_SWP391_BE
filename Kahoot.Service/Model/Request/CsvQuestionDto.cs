using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Model.Request
{
    public class CsvQuestionDto
    {
        public string QuestionText { get; set; }
        public string Answer1 { get; set; }
        public string Answer2 { get; set; }
        public string Answer3 { get; set; }
        public string Answer4 { get; set; }
        public int TimeLimitSec { get; set; }
        public string CorrectAnswers { get; set; }

        public IEnumerable<int> GetCorrectIndexes()
        {
            if (string.IsNullOrWhiteSpace(CorrectAnswers))
                return Enumerable.Empty<int>();

            return CorrectAnswers
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.Parse(s.Trim()));
        }
    }

}
