using Kahoot.Common.BusinessResult;
using Kahoot.Common;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using Kahoot.Service.Helpers;
using Kahoot.Service.Interface;
using Kahoot.Service.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Kahoot.Service.ModelDTOs.Request;
using Kahoot.Service.ModelDTOs.Response;
using Mapster;

namespace Kahoot.Service.Services
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QuizService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork ??= unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserIdClaim()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task<IBusinessResult> FindQuizById(int id)
        {
            var quiz = await _unitOfWork.QuizRepository
                .GetByWhere(q => q.QuizId == id)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync();
            if (quiz == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz not found");

            var response = quiz.Adapt<QuizResponse>();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Quiz found", response);
        }


        public async Task<IBusinessResult> CreateQuiz(QuizRequest request)
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            }
            var quiz = new Quiz();
            quiz.CreatedBy = int.Parse(userIdClaim);
            quiz.CreatedAt = DateTime.UtcNow;
            quiz.UpdateAt = DateTime.UtcNow;
            quiz.Description = request.Description;
            quiz.Title = request.Title;
            if (quiz.ImgUrl != null) {
                try
                {
                    var cloudinaryHelper = new CloudinaryHelper();
                    quiz.ImgUrl = await cloudinaryHelper.UploadImageWithCloudDinary(request.ImgUrl);
                }
                catch (Exception ex)
                {
                    return new BusinessResult(Const.ERROR_EXCEPTION, "Upload image error!");
                }
            }
            
            await _unitOfWork.QuizRepository.AddAsync(quiz);
            await _unitOfWork.SaveChangesAsync();
            return new BusinessResult(Const.HTTP_STATUS_OK, "Quiz created successfully", quiz);
        }
        public async Task<IBusinessResult> UpdateQuiz(int quizId, QuizRequest request)
        {
            var quiz = await _unitOfWork.QuizRepository
                .GetByWhere(q => q.QuizId == quizId)
                .FirstOrDefaultAsync();

            if (quiz == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz not found");

            quiz.Title = request.Title;
            quiz.Description = request.Description;
            if (quiz.ImgUrl != null)
            {
                try
                {
                    var cloudinaryHelper = new CloudinaryHelper();
                    quiz.ImgUrl = await cloudinaryHelper.UploadImageWithCloudDinary(request.ImgUrl);
                }
                catch (Exception ex)
                {
                    return new BusinessResult(Const.ERROR_EXCEPTION, "Upload image error!");
                }
            }
            quiz.UpdateAt = DateTime.UtcNow;

            await _unitOfWork.QuizRepository.UpdateAsync(quiz);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Quiz updated successfully", quiz);
        }

        public async Task<IBusinessResult> DeleteQuiz(int quizId)
        {
            var quiz = await _unitOfWork.QuizRepository
                .GetByWhere(x => x.QuizId == quizId)
                .FirstOrDefaultAsync();

            if (quiz == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz not found");
            }

            // Xóa quiz và commit thay đổi
            await _unitOfWork.QuizRepository.DeleteAsync(quiz);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Quiz deleted successfully");
        }
        public async Task<IBusinessResult> AddQuestionsToQuiz(int quizId, List<QuestionRequest> questionRequests)
        {
            // 1) Lấy userId
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            var userId = int.Parse(userIdClaim);

            // 2) Load quiz kèm navigation Questions→Answers
            var quiz = await _unitOfWork.QuizRepository
                .GetByWhere(q => q.QuizId == quizId && q.CreatedBy == userId)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync();

            if (quiz == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz not found");

            // 3) Xác định SortOrder bắt đầu
            var nextOrder = quiz.Questions.Any()
                ? quiz.Questions.Max(q => q.SortOrder) + 1
                : 1;

            foreach (var qr in questionRequests)
            {
                var question = new Question
                {
                    QuizId = quizId,
                    QuestionText = qr.QuestionText,
                    TimeLimitSec = qr.TimeLimitSec,
                    IsRandomAnswer = qr.IsRandomAnswer,
                    SortOrder = nextOrder++,
                    Createdat = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                };

                question.Answers = qr.Answers.Select(ar => new Answer
                {
                    AnswerText = ar.AnswerText,
                    IsCorrect = ar.IsCorrect,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                quiz.Questions.Add(question);
            }

            quiz.UpdateAt = DateTime.UtcNow;
            await _unitOfWork.QuizRepository.UpdateAsync(quiz);

            await _unitOfWork.SaveChangesAsync();
            return new BusinessResult(Const.HTTP_STATUS_OK, "Questions added successfully");
        }

        public async Task<IBusinessResult> UpdateQuestionsForQuiz(int quizId, List<QuestionRequest> questionRequests)
        {
            // 1) Lấy userId và quiz như trước
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            var userId = int.Parse(userIdClaim);

            var quiz = await _unitOfWork.QuizRepository
                .GetByWhere(q => q.QuizId == quizId && q.CreatedBy == userId)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync();

            if (quiz == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz không tồn tại hoặc không phải của bạn");

            quiz.Questions.Clear();

            int sortOrder = 1;
            foreach (var qr in questionRequests)
            {
                var question = new Question
                {
                    QuizId = quizId,
                    QuestionText = qr.QuestionText,
                    TimeLimitSec = qr.TimeLimitSec,
                    IsRandomAnswer = qr.IsRandomAnswer,
                    SortOrder = sortOrder++,
                    Createdat = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                };

                question.Answers = qr.Answers.Select(ar => new Answer
                {
                    AnswerText = ar.AnswerText,
                    IsCorrect = ar.IsCorrect,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                quiz.Questions.Add(question);
            }

            // 4) Cập nhật timestamp và save
            quiz.UpdateAt = DateTime.UtcNow;
            await _unitOfWork.QuizRepository.UpdateAsync(quiz);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Cập nhật câu hỏi và đáp án thành công");
        }


        public async Task<IBusinessResult> GetMyQuizzes()
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");

            int userId = int.Parse(userIdClaim);
            var quizzes = await _unitOfWork.QuizRepository
                .GetByWhere(q => q.CreatedBy == userId)
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .ToListAsync();
            var response = quizzes.Adapt<List<QuizResponse>>();
            return new BusinessResult(Const.HTTP_STATUS_OK, "OK", response);
        }

        public async Task<IBusinessResult> AddImageToQuestion(int questionId, ImageUpload request)
        {
            var question = await _unitOfWork.QuestionRepository
                .GetByWhere(q => q.QuestionId == questionId)
                .FirstOrDefaultAsync();

            if (question == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Question not found");

            try
            {
                var cloudinaryHelper = new CloudinaryHelper();
                question.ImgUrl = await cloudinaryHelper.UploadImageWithCloudDinary(request.ImgUrl);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION,
                    "Upload image failed: " + ex.Message);
            }
            question.UpdateAt = DateTime.UtcNow;
            await _unitOfWork.QuestionRepository.UpdateAsync(question);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Image added to question", question);
        }

        public async Task<IBusinessResult> DeleteQuestion(int questionId)
        {
            var question = await _unitOfWork.QuestionRepository
                .GetByWhere(q => q.QuestionId == questionId)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync();

            if (question == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Question not found");

            await _unitOfWork.QuestionRepository.DeleteAsync(question);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Question deleted successfully");
        }


    }
}
