using Kahoot.Repository.Models;
using Kahoot.Service.Interface;
using Kahoot.Service.Model.Request;
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
        [Authorize]
        public async Task<IActionResult> GetMyQuizzes()
        {
            var result = await _quizService.GetMyQuizzes();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetQuizById(int id)
        {
            var result = await _quizService.FindQuizById(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateQuiz([FromForm] QuizRequest request)
        {
            var result = await _quizService.CreateQuiz(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateQuiz(int id, [FromForm] QuizRequest request)
        {
            var result = await _quizService.UpdateQuiz(id, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/questions")]
        [Authorize]
        public async Task<IActionResult> AddQuestionsToQuiz(int id, [FromBody] List<QuestionRequest> questionRequests)
        {
            var result = await _quizService.AddQuestionsToQuiz(id, questionRequests);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/questions")]
        [Authorize]
        public async Task<IActionResult> UpdateQuestionsToQuiz(int id, [FromForm] List<QuestionRequest> questionRequests)
        {
            var result = await _quizService.UpdateQuestionsForQuiz(id, questionRequests);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var result = await _quizService.DeleteQuiz(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("questions/{questionId}/image")]
        [Authorize]
        public async Task<IActionResult> AddImageToQuestion(int questionId, [FromForm] ImageUpload request)
        {
            var result = await _quizService.AddImageToQuestion(questionId, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete("questions/{questionId}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var result = await _quizService.DeleteQuestion(questionId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut("{id}/questions/{questionId}/sortorder")]
        [Authorize]
        public async Task<IActionResult> SortQuestion(
            [FromRoute] int id,
            [FromRoute] int questionId,
            [FromBody] int sortOrder)
        {
            var result = await _quizService.SortOrderAsync(id, questionId, sortOrder);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("{quizId}/import")]
        [Authorize]
        public async Task<IActionResult> Import(int quizId, IFormFile file)
        {
            var result = await _quizService.ImportQuestionsFromFile(quizId, file);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet]
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}")]
        public async Task<IActionResult> Getquizlist(string? search, int pageNumber, int pageSize)
        {
            var result = await _quizService.GetAllQuizzesPaging(search, pageNumber,pageSize);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("generate-ai")]
        [Authorize]
        public async Task<IActionResult> GenerateQuizByAI([FromForm] CreateQuizAIRequest request)
        {
            var result = await _quizService.CreateQuizAI(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("generate-answer-ai")]
        [Authorize]
        public async Task<IActionResult> GenerateAnswersForQuestionAI([FromForm] string content)
        {
            var result = await _quizService.GenerateAnswersForQuestionAI(content);
            return StatusCode(result.StatusCode, result);
        }
        
    }
}
