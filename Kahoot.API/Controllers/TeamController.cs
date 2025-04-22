using System.Threading.Tasks;
using Kahoot.API.Hubs;
using Kahoot.Common.BusinessResult;
using Kahoot.Service.Interface;
using Kahoot.Service.Model.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Kahoot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Teacher,Player")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;
        private readonly IHubContext<GameHub> _hubContext;

        public TeamController(ITeamService teamService, IHubContext<GameHub> hubContext)
        {
            _teamService = teamService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Tạo mới một Team trong session
        /// POST api/team
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromBody] TeamRequest request)
        {
            var result = await _teamService.CreateTeamAsync(request);
            if (result.StatusCode >= 200 && result.StatusCode < 300)
            {
                // Broadcast sự kiện TeamCreated nếu cần
                // await _hubContext.Clients.Group(request.SessionCode).SendAsync("TeamCreated", result.Data);
            }
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy danh sách Teams theo SessionCode
        /// GET api/team/session/{sessionCode}
        /// </summary>
        [HttpGet("session/{sessionCode}")]
        public async Task<IActionResult> GetTeams([FromRoute] string sessionCode)
        {
            var result = await _teamService.GetTeamsAsync(sessionCode);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Người chơi tham gia Team
        /// POST api/team/{teamId}/join
        /// </summary>
        [HttpPost("{teamId}/join")]
        public async Task<IActionResult> JoinTeam([FromRoute] int teamId, [FromBody] JoinTeamRequest request)
        {
            // Đảm bảo teamId nhất quán
            if (request.teamId != teamId)
                request.teamId = teamId;

            var result = await _teamService.JoinTeamAsync(request);
            if (result.StatusCode >= 200 && result.StatusCode < 300)
            {
                // Broadcast sự kiện PlayerJoined nếu cần
                // await _hubContext.Clients.Group($"Session_{request.teamId}").SendAsync("PlayerJoined", result.Data);
            }
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy tổng điểm của Team
        /// GET api/team/{teamId}/score
        /// </summary>
        [HttpGet("{teamId}/score")]
        public async Task<IActionResult> GetTeamScore([FromRoute] int teamId)
        {
            var result = await _teamService.GetTeamScoreAsync(teamId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
