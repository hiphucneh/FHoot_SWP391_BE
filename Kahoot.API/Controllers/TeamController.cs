using System.Threading.Tasks;
using Azure.Core;
using Kahoot.API.Hubs;
using Kahoot.Common.BusinessResult;
using Kahoot.Service.Interface;
using Kahoot.Service.Model.Request;
using Kahoot.Service.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Kahoot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;
        private readonly IHubContext<GameHub, IGameHubClient> _hubContext;

        public TeamController(ITeamService teamService, IHubContext<GameHub, IGameHubClient> hubContext)
        {
            _teamService = teamService;
            _hubContext = hubContext;
        }
        [HttpPost]
        [Authorize(Roles = "Teacher,User")]
        public async Task<IActionResult> CreateTeam([FromBody] TeamRequest request)
        {
            var result = await _teamService.CreateTeamAsync(request);
            if (result.StatusCode >= 200 && result.StatusCode < 300)
            {
                var team = (TeamResponse)result.Data!;
                await _hubContext.Clients
                    .Group(request.SessionCode)
                    .TeamCreated(team);
            }
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("session/{sessionCode}")]
        [Authorize(Roles = "Teacher,User")]
        public async Task<IActionResult> GetTeams([FromRoute] string sessionCode)
        {
            var result = await _teamService.GetTeamsAsync(sessionCode);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("{SessionCode}/join")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> JoinTeam([FromRoute] string SessionCode, [FromForm] JoinTeamRequest request)
        {

            var result = await _teamService.JoinTeamAsync(request);
            if (result.StatusCode >= 200 && result.StatusCode < 300)
            {
                var player = (PlayerResponse)result.Data!;
                await _hubContext.Clients
                    .Group(SessionCode)
                    .PlayerJoined(player);
            }
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{teamId}/score")]
        [Authorize(Roles = "Teacher,User")]
        public async Task<IActionResult> GetTeamScore([FromRoute] int teamId)
        {
            var result = await _teamService.GetTeamScoreAsync(teamId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete("{SessionCode}/{teamId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteTeam([FromRoute] string SessionCode,  int teamId)
        {
            var result = await _teamService.DeleteTeamAsync(teamId);
            if (result.StatusCode >= 200 && result.StatusCode < 300)
            {
                var team = (TeamResponse)result.Data!;
                await _hubContext.Clients
                    .Group(SessionCode)
                    .TeamDelete(teamId);
            }
            return StatusCode(result.StatusCode, result);
        }
    }
}
