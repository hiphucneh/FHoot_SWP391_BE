using Kahoot.API.Hubs;
using Kahoot.Common.BusinessResult;
using Kahoot.Service.Interface;
using Kahoot.Service.Model.Request;
using Kahoot.Service.Model.Response;
using Kahoot.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Kahoot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService _playerService;
        private readonly IHubContext<GameHub, IGameHubClient> _hubContext;

        public PlayerController(IPlayerService playerService, IHubContext<GameHub, IGameHubClient> hubContext)
        {
            _playerService = playerService;
            _hubContext = hubContext;
        }
        [HttpPost("answer")]
        public async Task<IActionResult> AnswerQuestion([FromForm] AnswerQuestionRequest request)
        {
            IBusinessResult result = await _playerService.AnswerQuestionAsync(request);
            if (result.StatusCode < 200 || result.StatusCode >= 300)
                return StatusCode(result.StatusCode, result);
            var playeranswerdto = (AnswerTotalScoreResponse)result.Data!;
            await _hubContext.Clients
               .Group(request.SessionCode)
               .PlayerAnswer(playeranswerdto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
