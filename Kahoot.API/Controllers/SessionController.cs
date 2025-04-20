using Kahoot.API.Hubs;
using Kahoot.Service.Interface;
using Kahoot.Service.Model.Request;
using Kahoot.Service.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Kahoot.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Teacher,Player")]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly IHubContext<GameHub> _hubContext;

        public SessionController(ISessionService sessionService, IHubContext<GameHub> hubContext)
        {
            _sessionService = sessionService;
            _hubContext = hubContext;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSessionRequest request)
        {
            var result = await _sessionService.CreateSessionAsync(request);
            if (result.StatusCode != 200)
                return StatusCode(result.StatusCode, result.Message);

            var data = (CreateSessionResponse)result.Data;
            await _hubContext
                .Clients
                .Group(data.SessionCode)
                .SendAsync("SessionCreated", data);

            // 4. Trả HTTP 200 back cho caller
            return Ok(data);
        }


    }
}
