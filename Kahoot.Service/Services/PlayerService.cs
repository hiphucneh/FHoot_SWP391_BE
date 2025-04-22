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
using Kahoot.Service.Model.Response;

namespace Kahoot.Service.Services
{
    public class AnswerQuestionRequest
    {
        public int QuestionSessionId { get; set; }
        public int AnswerId { get; set; }
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
                score = (int)Math.Ceiling(1000.0 * (totalPlayers - answerOrder + 1) / totalPlayers);

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

            var totalScore = await _unitOfWork.PlayerAnswerRepository
                .GetByWhere(x => x.PlayerId == player.PlayerId && x.QuestionSession.SessionId == qs.SessionId)
                .SumAsync(x => x.Score);

            var response = new AnswerTotalScoreResponse
            {
                IsCorrect = isCorrect,
                Score = score,
                AnswerOrder = answerOrder,
                TotalScore = totalScore
            };
            return new BusinessResult(
                Const.HTTP_STATUS_OK,
                "Trả lời thành công",
                response
            );
        }
    }

}
