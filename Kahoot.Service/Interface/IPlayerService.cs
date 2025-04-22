using Kahoot.Common.BusinessResult;
using Kahoot.Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Interface
{
    public interface IPlayerService
    {
        Task<IBusinessResult> GetMySessionScoreAsync(string sessionCode);
        Task<IBusinessResult> AnswerQuestionAsync(AnswerQuestionRequest request);
    }
}
