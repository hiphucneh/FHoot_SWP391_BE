using Kahoot.Common.BusinessResult;
using Kahoot.Service.Model.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Interface
{
    public interface ISessionService
    {
        Task<IBusinessResult> CreateSessionAsync(CreateSessionRequest request);
        Task<IBusinessResult> StartSessionAsync(string sessionCode);
        Task<IBusinessResult> EndSessionAsync(string sessionCode);
        Task<IBusinessResult> GetMySessionsAsync();
        Task<IBusinessResult> GetSessionTeamLeaderboardAsync(string sessionCode);
        Task<IBusinessResult> NextQuestionAsync(string sessionCode, int sortOrder, int timeLimitSec);
        Task<IBusinessResult> GetAllSessionsAsync(int pageNumber = 1, int pageSize = 10, string? search = null);
    }
}
