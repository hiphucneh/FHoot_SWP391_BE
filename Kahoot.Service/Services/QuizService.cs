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
                .GetByWhere(x => x.QuizId == id)
                .Include(x => x.Questions)
                .FirstOrDefaultAsync();

            if (quiz == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz not found");
            }

            return new BusinessResult(Const.HTTP_STATUS_OK, "Quiz found", quiz);
        }

        public async Task<IBusinessResult> CreateQuiz(Quiz quiz)
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            }

            // Gán thông tin người tạo và thời gian tạo quiz
            quiz.CreatedBy = int.Parse(userIdClaim);
            quiz.CreatedDate = DateTime.UtcNow;

            // Thêm quiz vào repository và commit thay đổi
            await _unitOfWork.QuizRepository.AddAsync(quiz);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Quiz created successfully", quiz);
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

        public async Task<IBusinessResult> GetMyQuizzes()
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");

            int userId = int.Parse(userIdClaim);
            var quizzes = await _unitOfWork.QuizRepository
                .GetByWhere(q => q.CreatedBy == userId)
                .Include(q => q.Questions)
                .ToListAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Quizzes retrieved successfully", quizzes);
        }

    }
}
