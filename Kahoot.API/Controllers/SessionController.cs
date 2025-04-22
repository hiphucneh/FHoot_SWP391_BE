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
        private readonly IHubContext<GameHub, IGameHubClient> _hubContext;

        public SessionController(
            ISessionService sessionService,
            IHubContext<GameHub, IGameHubClient> hubContext)
        {
            _sessionService = sessionService;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            var result = await _sessionService.CreateSessionAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("my-session")]
        public async Task<IActionResult> GetMySessions()
        {
            var result = await _sessionService.GetMySessionsAsync();
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("{sessionCode}/finish")]
        public async Task<IActionResult> EndSession([FromRoute] string sessionCode)
        {
            var result = await _sessionService.EndSessionAsync(sessionCode);

            if (result.StatusCode >= 200 && result.StatusCode < 300)
            {
                await _hubContext.Clients.Group(sessionCode).SessionEnded();
            }

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{sessionCode}/start")]
        public async Task<IActionResult> StartSession([FromRoute] string sessionCode)
        {
            var result = await _sessionService.StartSessionAsync(sessionCode);

            if (result.StatusCode >= 200 && result.StatusCode < 300)
            {
                await _hubContext.Clients.Group(sessionCode).SessionStarted();
            }

            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("{sessionCode}/leaderboard")]
        public async Task<IActionResult> GetLeaderboard([FromRoute] string sessionCode)
        {
            var result = await _sessionService.GetSessionTeamLeaderboardAsync(sessionCode);
            return StatusCode(result.StatusCode, result);
        }
    }
}