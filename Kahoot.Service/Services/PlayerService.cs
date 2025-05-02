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
using Kahoot.Service.Model.Request;

namespace Kahoot.Service.Services
{


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
            var session = await _unitOfWork.SessionRepository
                .GetByWhere(s => s.SessionCode == request.SessionCode && s.EndAt == null)
                .Include(s => s.Quiz)
                .FirstOrDefaultAsync();
            if (session == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Session không tồn tại hoặc đã kết thúc");
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

            var trueAnswerId = await _unitOfWork.AnswerRepository
    .GetByWhere(a => a.QuestionId == qs.QuestionId && a.IsCorrect)
    .Select(a => a.AnswerId)
    .FirstOrDefaultAsync();

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
                TotalScore = totalScore,
                TrueAnswer = trueAnswerId
            };
            return new BusinessResult(
                Const.HTTP_STATUS_OK,
                "Trả lời thành công",
                response
            );
        }

        public async Task<IBusinessResult> GetPlayerResultInSession(string sessionCode)
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");

            var userId = int.Parse(userIdClaim);

            var session = await _unitOfWork.SessionRepository
                .GetByWhere(s => s.SessionCode == sessionCode)
                .FirstOrDefaultAsync();

            if (session == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Session không tồn tại");

            var player = await _unitOfWork.PlayerRepository
                .GetByWhere(p => p.UserId == userId && p.Team.SessionId == session.SessionId)
                .FirstOrDefaultAsync();

            if (player == null)
                return new BusinessResult(Const.HTTP_STATUS_FORBIDDEN, "Bạn chưa tham gia phiên chơi này");

            // 👉 Include QuestionSession -> Question và Answer
            var playerAnswers = await _unitOfWork.PlayerAnswerRepository
                .GetByWhere(pa => pa.PlayerId == player.PlayerId && pa.QuestionSession.SessionId == session.SessionId)
                .Include(pa => pa.QuestionSession)
                    .ThenInclude(qs => qs.Question)
                .Include(pa => pa.Answer)
                .Select(pa => new PlayerAnswerInfo
                {
                    QuestionSessionId = pa.QuestionSessionId,
                    AnswerId = pa.AnswerId,
                    IsCorrect = pa.IsCorrect,
                    Score = pa.Score,
                    AnswerOrder = pa.AnswerOrder,
                    QuestionText = pa.QuestionSession.Question.QuestionText,
                    AnswerText = pa.Answer.AnswerText
                })
                .ToListAsync();

            var totalScore = playerAnswers.Sum(x => x.Score);

            var result = new PlayerSessionResultResponse
            {
                PlayerId = player.PlayerId,
                TotalScore = totalScore,
                Answers = playerAnswers
            };

            return new BusinessResult(Const.HTTP_STATUS_OK, "Lấy kết quả thành công", result);
        }

        public async Task<IBusinessResult> GetAllSessionsOfUserAsync()
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");

            var userId = int.Parse(userIdClaim);

            var sessions = await _unitOfWork.PlayerRepository
                .GetByWhere(p => p.UserId == userId)
                .Include(p => p.Team)
                    .ThenInclude(t => t.Session)
                        .ThenInclude(s => s.Quiz)
                .Select(p => new UserSessionInfoResponse
                {
                    SessionName = p.Team.Session.SessionName,
                    SessionId = p.Team.Session.SessionId,
                    SessionCode = p.Team.Session.SessionCode,
                    EndAt = p.Team.Session.EndAt
                })
                .Distinct()
                .ToListAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Lấy danh sách phiên chơi thành công", sessions);
        }

    }

}
