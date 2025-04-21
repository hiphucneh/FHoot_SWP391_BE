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
        public async Task<IBusinessResult> CreateTeamAsync(TeamRequest request)
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            }
            var userid = int.Parse(userIdClaim);
            var session = await _unitOfWork.SessionRepository
                .GetByWhere(s => s.SessionCode == request.SessionCode)
                .Include(s => s.Teams)
                .FirstOrDefaultAsync();

            if (session == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Session không tồn tại");
            bool duplicate = session.Teams.Any(t => t.TeamName.Equals(request.TeamName, StringComparison.OrdinalIgnoreCase));
            if (duplicate)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Tên Team đã tồn tại trong phiên chơi");

            var now = DateTime.UtcNow;
            var team = new Team
            {
                SessionId = session.SessionId,
                TeamName = request.TeamName,
                TotalScore = 0,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _unitOfWork.TeamRepository.AddAsync(team);
            await _unitOfWork.SaveChangesAsync();

            var response = new TeamResponse
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                TotalScore = team.TotalScore,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt
            };

            return new BusinessResult(Const.HTTP_STATUS_OK, "Tạo Team thành công", response);
        }

        public async Task<IBusinessResult> GetTeams(string SessionCode)
        {
            var session = await _unitOfWork.SessionRepository
                .GetByWhere(s => s.SessionCode == SessionCode)
                .Include(s => s.Teams)
                .FirstOrDefaultAsync();

            var teams = session?.Teams;
            var response = teams.Adapt<TeamResponse>();
            return new BusinessResult(Const.HTTP_STATUS_OK, "Tạo Team thành công", response);
        }
        public async Task<IBusinessResult> JoinTeamAsync(int teamid)
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            var userId = int.Parse(userIdClaim);
            var team = await _unitOfWork.TeamRepository
                .GetByWhere(t => t.TeamId == teamid)
                .Include(t => t.Players)
                .FirstOrDefaultAsync();

            if (team == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Team không tồn tại");
            if (team.Players.Any(p => p.UserId == userId))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Bạn đã tham gia Team này");
            var now = DateTime.UtcNow;
            var player = new Player
            {
                TeamId = teamid,
                UserId = userId,
                JoinedAt = now
            };

            await _unitOfWork.PlayerRepository.AddAsync(player);
            await _unitOfWork.SaveChangesAsync();
            var response = new PlayerResponse
            {
                PlayerId = player.PlayerId,
                TeamId = player.TeamId,
                JoinedAt = player.JoinedAt
            };

            return new BusinessResult(Const.HTTP_STATUS_OK, "Gia nhập Team thành công", response);
        }
        public async Task<IBusinessResult> StartSessionAsync(string SessionCode)
        {
            var session = await _unitOfWork.SessionRepository
                .GetByWhere(s => s.SessionCode == SessionCode)
                .FirstOrDefaultAsync();

            if (session == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Session không tồn tại");

            var questions = await _unitOfWork.QuestionRepository
                .GetByWhere(q => q.QuizId == session.QuizId)
                .Include(q => q.Answers)
                .OrderBy(q => q.SortOrder)
                .ToListAsync();

            if (!questions.Any())
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz chưa có câu hỏi nào");


            return new BusinessResult(
                Const.HTTP_STATUS_OK,
                "Phiên chơi bắt đầu thành công",
                questions
            );
        }

    }
}