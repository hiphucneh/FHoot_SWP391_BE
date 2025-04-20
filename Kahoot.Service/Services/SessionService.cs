using System;
using System.Linq;
using System.Threading.Tasks;
using Kahoot.Common.BusinessResult;
using Kahoot.Common;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using Kahoot.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Kahoot.Service.Model.Request;
using Kahoot.Service.Model.Response;
using System.Security.Claims;

namespace Kahoot.Service.Services
{
    // SessionService chỉ chứa logic nghiệp vụ, không phụ thuộc vào SignalR/Hub
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionService(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var idClaim = user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }

        public async Task<IBusinessResult> CreateSessionAsync(CreateSessionRequest request)
        {
            // Xác thực và kiểm tra quyền (nếu cần)
            var userId = GetCurrentUserId();
            if (userId == 0) return new BusinessResult(Const.HTTP_STATUS_UNAUTHORIZED, "Unauthorized");

            // Kiểm tra Quiz tồn tại
            var quiz = await _unitOfWork.QuizRepository
                .GetByWhere(q => q.QuizId == request.QuizId)
                .FirstOrDefaultAsync();
            if (quiz == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz không tồn tại");

            // Tạo session
            string code = GenerateSessionCode(6);
            var session = new Session
            {
                QuizId = request.QuizId,
                SessionName = request.SessionName,
                SessionCode = code,
                CreatedAt = DateTime.UtcNow,
                EndedManually = false
            };
            await _unitOfWork.SessionRepository.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();

            var response = new CreateSessionResponse
            {
                SessionId = session.SessionId,
                QuizId = session.QuizId,
                SessionCode = session.SessionCode,
                SessionName = session.SessionName,
                CreatedAt = session.CreatedAt
            };

            return new BusinessResult(Const.HTTP_STATUS_OK, "Tạo phiên chơi thành công", response);
        }

        private string GenerateSessionCode(int length)
        {
            const string chars = "0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, length)
                .Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }
    }
}