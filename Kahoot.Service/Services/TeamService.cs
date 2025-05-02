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
using Mapster;
using Kahoot.Service.Utilities;
using Microsoft.IdentityModel.Tokens;

namespace Kahoot.Service.Services
{
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TeamService(
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

        public async Task<IBusinessResult> CreateTeamAsync(TeamRequest request)
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");

            var session = await _unitOfWork.SessionRepository
                .GetByWhere(s => s.SessionCode == request.SessionCode && s.EndAt == null)
                .Include(s => s.Teams)
                .FirstOrDefaultAsync();

            if (session == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Session không tồn tại hoặc đã kết thúc");

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
        public async Task<IBusinessResult> DeleteTeamAsync(int teamId)
        {
            var team = await _unitOfWork.TeamRepository
                .GetByWhere(t => t.TeamId == teamId)
                .Include(t => t.Players)
                .FirstOrDefaultAsync();

            if (team == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Team không tồn tại", null);
            }

            if (team.Players != null && team.Players.Any())
            {
                await _unitOfWork.PlayerRepository.RemoveRange(team.Players);
            }

            await _unitOfWork.TeamRepository.DeleteAsync(team);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return new BusinessResult(Const.HTTP_STATUS_OK, "Xóa Team thành công", null);
            }
            catch (DbUpdateException)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Có lỗi xảy ra khi xóa Team", null);
            }
        }
        public async Task<IBusinessResult> GetTeamsAsync(string sessionCode)
        {
            var session = await _unitOfWork.SessionRepository
                .GetByWhere(s => s.SessionCode == sessionCode)
                .Include(s => s.Teams)
                    .ThenInclude(t => t.Players)
                .FirstOrDefaultAsync();

            if (session == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Session không tồn tại", null);

            var teams = session.Teams;
            var response = teams.Adapt<List<TeamResponse>>();
            return new BusinessResult(Const.HTTP_STATUS_OK, "Lấy danh sách Team thành công", response);
        }
        public async Task<IBusinessResult> JoinTeamAsync(JoinTeamRequest request)
        {
            var team = await _unitOfWork.TeamRepository
                .GetByWhere(t => t.TeamId == request.teamId)
                .Include(t => t.Players)
                .FirstOrDefaultAsync();

            if (team == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Team không tồn tại", null);

            var userIdClaim = GetUserIdClaim();
            if (!int.TryParse(userIdClaim, out var userId))
                return new BusinessResult(Const.HTTP_STATUS_UNAUTHORIZED, "Không xác thực được user", null);

            if (team.Players.Any(p => p.UserId == userId))
                return new BusinessResult(Const.HTTP_STATUS_CONFLICT, "Bạn đã tham gia team này rồi", null);

            var newPlayer = new Player
            {
                TeamId = request.teamId,
                UserId = userId,
                Name = request.FullName,
                JoinedAt = DateTime.UtcNow
            };
            if (request.ImageUrl != null)
            {
                try
                {
                    var cloudinaryHelper = new CloudinaryHelper();
                    newPlayer.ImageUrl = await cloudinaryHelper.UploadImageWithCloudDinary(request.ImageUrl);
                }
                catch
                {
                    return new BusinessResult(Const.ERROR_EXCEPTION, "Upload image error!");
                }
            }

            await _unitOfWork.PlayerRepository.AddAsync(newPlayer);
            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Lỗi khi lưu dữ liệu, vui lòng thử lại sau");
            }

            var playerResponse = new PlayerResponse
            {
                PlayerId = newPlayer.PlayerId,
                Name = newPlayer.Name,
                TeamName = team.TeamName,
                ImageUrl = newPlayer.ImageUrl,
                Score = 0,
                JoinedAt = newPlayer.JoinedAt
            };

            return new BusinessResult(Const.HTTP_STATUS_OK, "Tham gia team thành công", playerResponse);
        }
        public async Task<IBusinessResult> GetTeamScoreAsync(int teamId)
        {
            var team = await _unitOfWork.TeamRepository
                .GetByWhere(t => t.TeamId == teamId)
                .Include(t => t.Players)
                .FirstOrDefaultAsync();

            if (team == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Team không tồn tại", null);

            if (team.Players == null || !team.Players.Any())
            {
                return new BusinessResult(Const.HTTP_STATUS_OK, "Team chưa có thành viên nào", new
                {
                    TeamId = teamId,
                    TotalScore = 0,
                    Rank = 0,
                    Players = new List<object>()
                });
            }

            var playerIds = team.Players.Select(p => p.PlayerId).ToList();

            var playerScores = await _unitOfWork.PlayerAnswerRepository
                .GetByWhere(pa => playerIds.Contains(pa.PlayerId))
                .GroupBy(pa => pa.PlayerId)
                .Select(g => new
                {
                    PlayerId = g.Key,
                    TotalScore = g.Sum(pa => pa.Score)
                })
                .ToListAsync();

            var playersWithScore = team.Players.Select(p =>
            {
                var playerScore = playerScores.FirstOrDefault(ps => ps.PlayerId == p.PlayerId);
                return new
                {
                    p.PlayerId,
                    p.Name,
                    p.ImageUrl,
                    Score = playerScore?.TotalScore ?? 0
                };
            }).ToList();

            var totalTeamScore = playersWithScore.Sum(p => p.Score);

            var sessionId = team.SessionId;
            var allTeams = await _unitOfWork.TeamRepository
                .GetByWhere(t => t.SessionId == sessionId)
                .Include(t => t.Players)
                .ToListAsync();

            var teamRankList = new List<(int TeamId, int TotalScore)>();

            foreach (var t in allTeams)
            {
                var tPlayerIds = t.Players.Select(p => p.PlayerId).ToList();
                var tScore = await _unitOfWork.PlayerAnswerRepository
                    .GetByWhere(pa => tPlayerIds.Contains(pa.PlayerId))
                    .SumAsync(pa => pa.Score);

                teamRankList.Add((t.TeamId, tScore));
            }

            var rankedList = teamRankList
                .OrderByDescending(x => x.TotalScore)
                .Select((item, index) => new { item.TeamId, Rank = index + 1 })
                .ToList();

            var currentTeamRank = rankedList.FirstOrDefault(r => r.TeamId == teamId)?.Rank ?? 0;

            var response = new
            {
                TeamId = teamId,
                TeamName = team.TeamName,
                TotalScore = totalTeamScore,
                Rank = currentTeamRank,
                Players = playersWithScore
            };

            return new BusinessResult(Const.HTTP_STATUS_OK, "Lấy điểm Team thành công", response);
        }


    }
}
