using Kahoot.Common.BusinessResult;
using Kahoot.Common;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using Kahoot.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Services
{
    public class AnswerQuestionRequest
    {
        public int QuestionSessionId { get; set; }
        public int AnswerId { get; set; }
    }

    public class AnswerQuestionResponse
    {
        public bool IsCorrect { get; set; }
        public int Score { get; set; }
        public int AnswerOrder { get; set; }
    }

    public class PlayerService : IPlayerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PlayerService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserIdClaim()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task<IBusinessResult> AnswerQuestionAsync(AnswerQuestionRequest request)
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            var userId = int.Parse(userIdClaim);

            var qs = await _unitOfWork.QuestionSessionRepository
                .GetByWhere(x => x.QuestionSessionId == request.QuestionSessionId)
                .Include(x => x.Session)
                .FirstOrDefaultAsync();
            if (qs == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "QuestionSession không tồn tại");

            var player = await _unitOfWork.PlayerRepository
                .GetByWhere(p => p.UserId == userId && p.Team.SessionId == qs.SessionId)
                .FirstOrDefaultAsync();
            if (player == null)
                return new BusinessResult(Const.HTTP_STATUS_FORBIDDEN, "Bạn chưa tham gia phiên chơi này");

            bool already = await _unitOfWork.PlayerAnswerRepository
                .GetByWhere(pa => pa.PlayerId == player.PlayerId && pa.QuestionSessionId == request.QuestionSessionId)
                .AnyAsync();
            if (already)
                return new BusinessResult(Const.HTTP_STATUS_CONFLICT, "Bạn đã trả lời câu hỏi này rồi");

            var answer = await _unitOfWork.AnswerRepository
                .GetByWhere(a => a.AnswerId == request.AnswerId && a.QuestionId == qs.QuestionId)
                .FirstOrDefaultAsync();
            if (answer == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Answer không hợp lệ");

            bool isCorrect = answer.IsCorrect;

            int existingCount = await _unitOfWork.PlayerAnswerRepository
                .GetByWhere(pa => pa.QuestionSessionId == request.QuestionSessionId)
                .CountAsync();
            int answerOrder = existingCount + 1;

            int totalPlayers = await _unitOfWork.PlayerRepository
                .GetByWhere(p => p.Team.SessionId == qs.SessionId)
                .CountAsync();
            int score = 0;
            if (isCorrect)
            {
                score = (int)Math.Ceiling(1000.0 * (totalPlayers - answerOrder + 1) / totalPlayers);
            }

            // 9) Tạo và lưu PlayerAnswer
            var pa = new PlayerAnswer
            {
                PlayerId = player.PlayerId,
                QuestionSessionId = request.QuestionSessionId,
                AnswerId = request.AnswerId,
                AnswerTime = DateTime.UtcNow,
                AnswerOrder = answerOrder,
                IsCorrect = isCorrect,
                Score = score
            };
            await _unitOfWork.PlayerAnswerRepository.AddAsync(pa);
            await _unitOfWork.SaveChangesAsync();

            // 10) Trả về kết quả
            var response = new AnswerQuestionResponse
            {
                IsCorrect = isCorrect,
                Score = score,
                AnswerOrder = answerOrder
            };
            return new BusinessResult(Const.HTTP_STATUS_OK, "Trả lời thành công", response);
        }
        public async Task<IBusinessResult> GetMySessionScoreAsync(string sessionCode)
        {
            // 1) Xác thực user
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            var userId = int.Parse(userIdClaim);

            // 2) Lấy session để xác định sessionId
            var session = await _unitOfWork.SessionRepository
                .GetByWhere(s => s.SessionCode == sessionCode)
                .FirstOrDefaultAsync();
            if (session == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Session không tồn tại");

            // 3) Lấy player tương ứng trong session
            var player = await _unitOfWork.PlayerRepository
                .GetByWhere(p => p.UserId == userId && p.Team.SessionId == session.SessionId)
                .FirstOrDefaultAsync();
            if (player == null)
                return new BusinessResult(Const.HTTP_STATUS_FORBIDDEN, "Bạn chưa tham gia phiên chơi này");

            // 4) Tính tổng điểm từ bảng PlayerAnswer
            var totalScore = await _unitOfWork.PlayerAnswerRepository
                .GetByWhere(pa => pa.PlayerId == player.PlayerId && pa.QuestionSession.SessionId == session.SessionId)
                .SumAsync(pa => pa.Score);

            // 5) Trả về kết quả
            var response = new
            {
                PlayerId = player.PlayerId,
                SessionId = session.SessionId,
                TotalScore = totalScore
            };

            return new BusinessResult(Const.HTTP_STATUS_OK, "Lấy tổng điểm thành công", response);
        }

    }

}
