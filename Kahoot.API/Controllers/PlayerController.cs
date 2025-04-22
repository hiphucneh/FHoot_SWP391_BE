using Kahoot.Common.BusinessResult;
using Kahoot.Service.Interface;
using Kahoot.Service.Model.Request;
using Kahoot.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Kahoot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        public PlayerController(IPlayerService playerService)
        {
            _playerService = playerService;
        }
        [HttpPost("answer")]
        public async Task<IActionResult> AnswerQuestion([FromForm] AnswerQuestionRequest request)
        {
            IBusinessResult result = await _playerService.AnswerQuestionAsync(request);
            return StatusCode(result.StatusCode, result);
        }
    }
}
