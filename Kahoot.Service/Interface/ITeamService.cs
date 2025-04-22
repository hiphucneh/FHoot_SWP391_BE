using Kahoot.Common.BusinessResult;
using Kahoot.Service.Model.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Interface
{
    public interface ITeamService
    {
        Task<IBusinessResult> CreateTeamAsync(TeamRequest request);
        Task<IBusinessResult> JoinTeamAsync(JoinTeamRequest teamId);
        Task<IBusinessResult> GetTeamsAsync(string SessionCode);
        Task<IBusinessResult> GetTeamScoreAsync(int teamId);
    }
}
