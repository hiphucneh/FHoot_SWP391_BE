using Kahoot.API.Hubs;
using Kahoot.Common.BusinessResult;
using Kahoot.Service.Interface;
using Kahoot.Service.Model.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Kahoot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Teacher,Player")]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly IHubContext<GameHub> _hubContext;

        public SessionController(
            ISessionService sessionService,
            IHubContext<GameHub> hubContext)
        {
            _sessionService = sessionService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Tạo mới một session
        /// POST api/session
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            var result = await _sessionService.CreateSessionAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("team")]
        public async Task<IActionResult> CreateTeam([FromBody] TeamRequest request)
        {
            var result = await _sessionService.CreateTeamAsync(request);
            // Có thể broadcast event lên SignalR nếu cần:
            // if (result.IsSuccess) 
            //    await _hubContext.Clients.Group(request.SessionCode).SendAsync("TeamCreated", result.Data);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Người chơi tham gia vào team
        /// POST api/session/team/{teamId}/join
        /// </summary>
        [HttpPost("team/{teamId}/join")]
        public async Task<IActionResult> JoinTeam([FromRoute] int teamId)
        {
            var result = await _sessionService.JoinTeamAsync(teamId);
            // Nếu thành công, có thể thông báo cho host:
            // if (result.IsSuccess)
            //     await _hubContext.Clients.Group($"Session_{result.Data.SessionCode}")
            //         .SendAsync("PlayerJoined", result.Data);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Bắt đầu session theo mã code
        /// POST api/session/{sessionCode}/start
        /// </summary>
        [HttpPost("{sessionCode}/start")]
        public async Task<IActionResult> StartSession([FromRoute] string sessionCode)
        {
            var result = await _sessionService.StartSessionAsync(sessionCode);
            // Nếu thành công, broadcast sự kiện bắt đầu:
            // if (result.IsSuccess)
            //     await _hubContext.Clients.Group(sessionCode).SendAsync("SessionStarted");
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy danh sách teams của một session
        /// GET api/session/{sessionCode}/teams
        /// </summary>
        [HttpGet("{sessionCode}/teams")]
        public async Task<IActionResult> GetTeams([FromRoute] string sessionCode)
        {
            var result = await _sessionService.GetTeams(sessionCode);
            return StatusCode(result.StatusCode, result);
        }
    }
}
