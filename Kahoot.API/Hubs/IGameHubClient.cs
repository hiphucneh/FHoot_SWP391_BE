using System.Threading.Tasks;
using Kahoot.Service.Model.Response;

namespace Kahoot.API.Hubs
{
    public interface IGameHubClient
    {
        Task TeamCreated(TeamResponse team);
        Task TeamDelete(int teamId);
        Task PlayerJoined(PlayerResponse player);
        Task SessionStarted();
        Task SessionEnded();
        Task ShowQuestion(QuestionSessionResponse question);
        Task PlayerAnswer(AnswerTotalScoreResponse answerTotalScoreResponse);

    }
}