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
        Task<IBusinessResult> CreateTeamAsync(TeamRequest request);
        Task<IBusinessResult> JoinTeamAsync(int teamId);
        Task<IBusinessResult> StartSessionAsync(string sessionCode);
        Task<IBusinessResult> GetTeams(string SessionCode);
    }
}
