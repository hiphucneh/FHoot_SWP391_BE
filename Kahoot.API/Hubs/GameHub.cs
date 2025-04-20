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

        /// <summary>
        /// Host gọi để tạo và khởi động một phiên chơi mới
        /// </summary>
        public async Task CreateSession(CreateSessionRequest request)
        {
            // Gọi vào business layer để tạo session
            var result = await _sessionService.CreateSessionAsync(request);
            if (result.StatusCode != Const.HTTP_STATUS_OK)
            {
                await Clients.Caller.SendAsync("Error", result.Message);
                return;
            }

            // Lấy dữ liệu phản hồi
            var response = (CreateSessionResponse)result.Data;

            // Thêm connection của host vào nhóm session theo sessionCode
            await Groups.AddToGroupAsync(Context.ConnectionId, response.SessionCode);

            // Thông báo cho cả nhóm (hiện tại chỉ có host) biết đã tạo phiên
            await Clients.Group(response.SessionCode)
                         .SendAsync("SessionCreated", new
                         {
                             SessionName = response.SessionName,
                             SessionCode = response.SessionCode
                         });
        }
    }
}
