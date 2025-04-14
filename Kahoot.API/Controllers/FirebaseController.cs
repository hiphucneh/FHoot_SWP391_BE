using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kahoot.Service.Utilities;

namespace Kahoot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FirebaseController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;
        public FirebaseController(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        [HttpPost("send-notification")]
        public async Task<IActionResult> SendNotification([FromBody] string fcmToken, string? title, string body)
        {
            await _firebaseService.SendNotification(fcmToken, title, body);
            return Ok();
        }

        [HttpPost("enable-reminder")]
        [Authorize]
        public async Task<IActionResult> EnableReminder()
        {
            var result = await _firebaseService.EnableReminder();
            return StatusCode(result.StatusCode, result.Message);
        }
    }
}