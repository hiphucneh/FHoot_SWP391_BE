using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kahoot.Common;
using Kahoot.Common.BusinessResult;
using Kahoot.Service.Interface;
using Kahoot.Service.Model.Request;
using Kahoot.Service.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Kahoot.API.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        private readonly ISessionService _sessionService;

        public GameHub(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }
        public async Task CreateSession(CreateSessionRequest request)
        {
            var result = await _sessionService.CreateSessionAsync(request);
            if (!(result is BusinessResult br) || br.StatusCode != Const.HTTP_STATUS_OK)
            {
                await Clients.Caller.SendAsync("Error", result.Message);
                return;
            }

            var session = (SessionResponse)br.Data!;
            // Host join vào nhóm session này
            await Groups.AddToGroupAsync(Context.ConnectionId, session.SessionCode);
            // Gửi lại cho chính host
            await Clients.Caller.SendAsync("SessionCreated", session);
        }

        /// <summary>
        /// Host gọi để tạo Team mới trong phiên.
        /// </summary>
        public async Task CreateTeam(TeamRequest request)
        {
            var result = await _sessionService.CreateTeamAsync(request);
            if (!(result is BusinessResult br) || br.StatusCode != Const.HTTP_STATUS_OK)
            {
                await Clients.Caller.SendAsync("Error", result.Message);
                return;
            }

            var team = (TeamResponse)br.Data!;
            // Broadcast đến mọi kết nối đã join session đó
            await Clients.Group(request.SessionCode).SendAsync("TeamCreated", team);
        }

        /// <summary>
        /// Player gọi để gia nhập Team (biến thành Player).
        /// Cần truyền vào cả teamId và sessionCode để hub biết group nào mà add connection vào.
        /// </summary>
        public async Task JoinTeam(int teamId, string sessionCode)
        {
            var result = await _sessionService.JoinTeamAsync(teamId);
            if (!(result is BusinessResult br) || br.StatusCode != Const.HTTP_STATUS_OK)
            {
                await Clients.Caller.SendAsync("Error", result.Message);
                return;
            }

            var player = (PlayerResponse)br.Data!;
            // Add connection này vào nhóm chung của session
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionCode);
            // (Tuỳ chọn) Add vào nhóm riêng của team để có thể broadcast team-specific
            await Groups.AddToGroupAsync(Context.ConnectionId, $"team-{teamId}");

            // Thông báo cho host và các player khác trong session
            await Clients.Group(sessionCode).SendAsync("PlayerJoined", player);
        }

        /// <summary>
        /// Host gọi để bắt đầu phiên chơi, load toàn bộ câu hỏi kèm đáp án.
        /// </summary>
        public async Task StartSession(string sessionCode)
        {
            var result = await _sessionService.StartSessionAsync(sessionCode);
            if (!(result is BusinessResult br) || br.StatusCode != Const.HTTP_STATUS_OK)
            {
                await Clients.Caller.SendAsync("Error", result.Message);
                return;
            }

            var questions = (IEnumerable<QuestionSessionResponse>)br.Data!;
            // Broadcast danh sách câu hỏi + đáp án tới tất cả trong session
            await Clients.Group(sessionCode).SendAsync("SessionStarted", questions);
        }
    }
}
