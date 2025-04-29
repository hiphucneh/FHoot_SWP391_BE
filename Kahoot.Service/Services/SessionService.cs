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
using Kahoot.Service.ModelDTOs.Response;
using Mapster;
using Kahoot.Service.Utilities;
using System.Linq.Expressions;

namespace Kahoot.Service.Services
{
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
        private string GetUserIdClaim()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        public async Task<IBusinessResult> CreateSessionAsync(CreateSessionRequest request)
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            }
            var userid = int.Parse(userIdClaim);
            var quiz = await _unitOfWork.QuizRepository
                .GetByWhere(q => q.QuizId == request.QuizId && q.CreatedBy == userid)
                .FirstOrDefaultAsync();
            if (quiz == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz không tồn tại");

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

            var response = new SessionResponse
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

        public async Task<IBusinessResult> StartSessionAsync(string sessionCode)
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            var userId = int.Parse(userIdClaim);

            var session = await _unitOfWork.SessionRepository
                .GetByWhere(s => s.SessionCode == sessionCode && s.EndAt == null)
                .Include(s => s.Quiz)
                .FirstOrDefaultAsync();
            if (session == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Session không tồn tại hoặc đã kết thúc");
            if (session.Quiz.CreatedBy != userId)
                return new BusinessResult(Const.HTTP_STATUS_FORBIDDEN, "Bạn không có quyền bắt đầu phiên chơi này");
            var questions = await _unitOfWork.QuestionRepository
                .GetByWhere(q => q.QuizId == session.QuizId)
                .Include(q => q.Answers)
                .OrderBy(q => q.SortOrder)
                .ToListAsync();
            if (!questions.Any())
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz chưa có câu hỏi nào");
            var now = DateTime.UtcNow;
            var response = questions.Select(q => new QuestionSessionResponse
            {
                QuestionSessionId = 0,               
                Question = q.Adapt<QuestionResponse>(),
                SortOrder = q.SortOrder,
                TimeLimitSec = q.TimeLimitSec,
                RunAt = now
            }).ToList();

            return new BusinessResult(
                Const.HTTP_STATUS_OK,
                "Phiên chơi bắt đầu thành công",
                response
            );
        }
        public async Task<IBusinessResult> NextQuestionAsync(string sessionCode, int sortOrder)
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập");

            var userId = int.Parse(userIdClaim);
            var session = await _unitOfWork.SessionRepository
                .GetByWhere(s => s.SessionCode == sessionCode && s.EndAt == null)
                .Include(s => s.Quiz)
                .FirstOrDefaultAsync();
            if (session == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Session không tồn tại");
            if (session.Quiz.CreatedBy != userId)
                return new BusinessResult(Const.HTTP_STATUS_FORBIDDEN, "Bạn không có quyền");

            var question = await _unitOfWork.QuestionRepository
                .GetByWhere(q => q.QuizId == session.QuizId && q.SortOrder == sortOrder)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync();
            if (question == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, $"Không tìm thấy câu hỏi thứ {sortOrder}");

            var now = DateTime.UtcNow;
            var qs = new QuestionSession
            {
                SessionId = session.SessionId,
                QuestionId = question.QuestionId,
                SortOrder = sortOrder,
                RunAt = now
            };
            await _unitOfWork.QuestionSessionRepository.AddAsync(qs);
            await _unitOfWork.SaveChangesAsync();

            var dto = new QuestionSessionResponse
            {
                QuestionSessionId = qs.QuestionSessionId,
                SortOrder = sortOrder,
                RunAt = now,
                Question = question.Adapt<QuestionResponse>()
            };

            return new BusinessResult(Const.HTTP_STATUS_OK, "Tạo phiên hỏi kế tiếp thành công", dto);
        }
        public async Task<IBusinessResult> GetMySessionsAsync()
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            var userId = int.Parse(userIdClaim);

            var sessions = await _unitOfWork.SessionRepository
                .GetByWhere(s => s.Quiz.CreatedBy == userId)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();

            var response = sessions.Adapt<List<SessionResponse>>();
            return new BusinessResult(
                Const.HTTP_STATUS_OK,
                "Lấy danh sách phiên chơi thành công",
                response
            );
        }
        public async Task<IBusinessResult> EndSessionAsync(string sessionCode)
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            var userId = int.Parse(userIdClaim);
            var session = await _unitOfWork.SessionRepository
                .GetByWhere(s => s.SessionCode == sessionCode)
                .Include(s => s.Quiz)
                .FirstOrDefaultAsync();

            if (session == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Session không tồn tại");

            if (session.Quiz.CreatedBy != userId)
                return new BusinessResult(Const.HTTP_STATUS_FORBIDDEN, "Bạn không có quyền kết thúc phiên chơi này");

            if (session.EndedManually)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Phiên chơi đã được kết thúc trước đó");
            session.EndedManually = true;
            session.EndAt = DateTime.UtcNow;
            await _unitOfWork.SessionRepository.UpdateAsync(session);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Kết thúc phiên chơi thành công");
        }
        public async Task<IBusinessResult> GetSessionTeamLeaderboardAsync(string sessionCode)
        {
            var session = await _unitOfWork.SessionRepository
                .GetByWhere(s => s.SessionCode == sessionCode)
                .FirstOrDefaultAsync();
            if (session == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Session không tồn tại");

            var teams = await _unitOfWork.TeamRepository
                .GetByWhere(t => t.SessionId == session.SessionId)
                .Include(t => t.Players)
                    .ThenInclude(p => p.PlayerAnswers)
                .ToListAsync();

            var leaderboard = teams
                .Select(t => new
                {
                    t.TeamId,
                    t.TeamName,
                    Players = t.Players.Select(p => new
                    {
                        p.PlayerId,
                        p.Name,
                        TotalScore = p.PlayerAnswers.Sum(pa => pa.Score)
                    }).ToList(),
                    TotalScore = t.Players.SelectMany(p => p.PlayerAnswers).Sum(pa => pa.Score)
                })
                .OrderByDescending(x => x.TotalScore)
                .Select((x, idx) => new TeamLeaderboardItem
                {
                    TeamId = x.TeamId,
                    TeamName = x.TeamName,
                    TotalScore = x.TotalScore,
                    Rank = idx + 1,
                    Players = x.Players.Select(p => new PlayerLeaderboardItem
                    {
                        PlayerId = p.PlayerId,
                        Name = p.Name,
                        TotalScore = p.TotalScore
                    }).ToList()
                })
                .ToList();

            return new BusinessResult(
                Const.HTTP_STATUS_OK,
                "Lấy bảng xếp hạng Team thành công",
                leaderboard
            );
        }

        public async Task<IBusinessResult> GetAllSessionsAsync(int pageNumber = 1, int pageSize = 10, string? search = null)
        {
            Expression<Func<Session, bool>> predicate = s => true;

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                predicate = s =>
                    s.SessionName.ToLower().Contains(search) ||
                    s.SessionCode.ToLower().Contains(search);
            }

            var sessions = await _unitOfWork.SessionRepository.GetPagedAsync(
                pageNumber,
                pageSize,
                predicate: predicate,
                orderBy: q => q.OrderByDescending(s => s.CreatedAt),
                include: q => q.Include(s => s.Quiz)
            );

            var response = sessions.Select(s => new SessionResponse
            {
                SessionId = s.SessionId,
                SessionName = s.SessionName,
                SessionCode = s.SessionCode,
                QuizId = s.QuizId,
                CreatedAt = s.CreatedAt,
                EndAt = s.EndAt,
            }).ToList();

            return new BusinessResult(
                Const.HTTP_STATUS_OK,
                "Lấy danh sách tất cả phiên chơi thành công",
                response
            );
        }

    }
}