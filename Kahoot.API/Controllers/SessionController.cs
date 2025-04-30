using Kahoot.API.Hubs;
using Kahoot.Common.BusinessResult;
using Kahoot.Service.Interface;
using Kahoot.Service.Model.Request;
using Kahoot.Service.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Kahoot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            var result = await _sessionService.CreateSessionAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("my-session")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMySessions()
        {
            var result = await _sessionService.GetMySessionsAsync();
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("{sessionCode}/finish")]
        [Authorize(Roles = "Teacher")]
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
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> StartSession([FromRoute] string sessionCode)
        {
            var result = await _sessionService.StartSessionAsync(sessionCode);

            if (result.StatusCode >= 200 && result.StatusCode < 300)
            {
                await _hubContext.Clients.Group(sessionCode).SessionStarted();
            }

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{sessionCode}/next-question")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> NextQuestion(
       [FromRoute] string sessionCode,
       [FromQuery] int sortOrder,
       [FromQuery] int timeLimitSec)
        {
            var svcResult = await _sessionService.NextQuestionAsync(sessionCode, sortOrder, timeLimitSec);
            if (svcResult.StatusCode < 200 || svcResult.StatusCode >= 300)
                return StatusCode(svcResult.StatusCode, svcResult);

            var questionDto = (QuestionSessionResponse)svcResult.Data!;

            await _hubContext.Clients
                .Group(sessionCode)
                .ShowQuestion(questionDto);


            return StatusCode(svcResult.StatusCode, svcResult);
        }

        [HttpGet("{sessionCode}/leaderboard")]
        [Authorize(Roles = "Teacher,User,Admin")]
        public async Task<IActionResult> GetLeaderboard([FromRoute] string sessionCode)
        {
            var result = await _sessionService.GetSessionTeamLeaderboardAsync(sessionCode);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("list")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Getsessionlist(int pageNumber = 1, int pageSize = 10, string? search = null)
        {
            var result = await _sessionService.GetAllSessionsAsync(pageNumber,pageSize,search);
            return StatusCode(result.StatusCode, result);
        }

    }
}