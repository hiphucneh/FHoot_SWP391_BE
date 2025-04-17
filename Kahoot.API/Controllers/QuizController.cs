using Kahoot.Repository.Models;
using Kahoot.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriDiet.Service.Enums;

namespace Kahoot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        [HttpGet("my")]
        [Authorize(Roles = $"{nameof(RoleEnum.Teacher)}")]
        public async Task<IActionResult> GetMyQuizzes()
        {
            var result = await _quizService.GetMyQuizzes();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetQuizById(int id)
        {
            var result = await _quizService.FindQuizById(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateQuiz([FromBody] Quiz quiz)
        {
            var result = await _quizService.CreateQuiz(quiz);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var result = await _quizService.DeleteQuiz(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
