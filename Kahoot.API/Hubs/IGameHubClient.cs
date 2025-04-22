using System.Threading.Tasks;
using Kahoot.Service.Model.Response;

namespace Kahoot.API.Hubs
{
    public interface IGameHubClient
    {
        Task TeamCreated(TeamResponse team);
        Task PlayerJoined(PlayerResponse player);
        Task SessionStarted();
        Task SessionEnded();
    }
}