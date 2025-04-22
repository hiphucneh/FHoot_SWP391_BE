using Kahoot.Repository.Models;
using Kahoot.Service.Interface;
using Kahoot.Service.ModelDTOs.Request;
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

        [HttpGet("my-quiz")]
        [Authorize(Roles = $"{nameof(RoleEnum.Teacher)}")]
        public async Task<IActionResult> GetMyQuizzes()
        {
            var result = await _quizService.GetMyQuizzes();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{nameof(RoleEnum.Teacher)}")]
        public async Task<IActionResult> GetQuizById(int id)
        {
            var result = await _quizService.FindQuizById(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(RoleEnum.Teacher)}")]
        public async Task<IActionResult> CreateQuiz([FromForm] QuizRequest request)
        {
            var result = await _quizService.CreateQuiz(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{nameof(RoleEnum.Teacher)}")]
        public async Task<IActionResult> UpdateQuiz(int id, [FromForm] QuizRequest request)
        {
            var result = await _quizService.UpdateQuiz(id, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/questions")]
        [Authorize(Roles = $"{nameof(RoleEnum.Teacher)}")]
        public async Task<IActionResult> AddQuestionsToQuiz(int id, [FromBody] List<QuestionRequest> questionRequests)
        {
            var result = await _quizService.AddQuestionsToQuiz(id, questionRequests);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/questions")]
        [Authorize(Roles = $"{nameof(RoleEnum.Teacher)}")]
        public async Task<IActionResult> UpdateQuestionsToQuiz(int id, [FromBody] List<QuestionRequest> questionRequests)
        {
            var result = await _quizService.UpdateQuestionsForQuiz(id, questionRequests);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{nameof(RoleEnum.Teacher)}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var result = await _quizService.DeleteQuiz(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("questions/{questionId}/image")]
        [Authorize(Roles = $"{nameof(RoleEnum.Teacher)}")]
        public async Task<IActionResult> AddImageToQuestion(int questionId, [FromForm] ImageUpload request)
        {
            var result = await _quizService.AddImageToQuestion(questionId, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete("questions/{questionId}")]
        [Authorize(Roles = $"{nameof(RoleEnum.Teacher)}")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var result = await _quizService.DeleteQuestion(questionId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut("{id}/questions/{questionId}/sortorder")]
        [Authorize(Roles = $"{nameof(RoleEnum.Teacher)}")]
        public async Task<IActionResult> SortQuestion(
            [FromRoute] int id,
            [FromRoute] int questionId,
            [FromBody] int sortOrder)
        {
            var result = await _quizService.SortOrderAsync(id, questionId, sortOrder);
            return StatusCode(result.StatusCode, result);
        }
    }
}
