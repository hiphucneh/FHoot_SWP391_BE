using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Kahoot.API.Hubs
{
    [Authorize]
    public class GameHub : Hub<IGameHubClient>
    {
        public Task JoinSession(string sessionCode)
        => Groups.AddToGroupAsync(Context.ConnectionId, sessionCode);
    }
}