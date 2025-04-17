using Kahoot.Common.BusinessResult;
using Kahoot.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Interface
{
    public interface IQuizService
    {
        Task<IBusinessResult> FindQuizById(int id);
        Task<IBusinessResult> CreateQuiz(Quiz quiz);
        Task<IBusinessResult> DeleteQuiz(int quizId);
        Task<IBusinessResult> GetMyQuizzes();
    }
}
