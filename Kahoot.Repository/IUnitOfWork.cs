using System.Threading.Tasks;

namespace Kahoot.Repository.Interface
{
    public interface IUnitOfWork
    {
        Task BeginTransaction();
        Task CommitTransaction();
        Task RollbackTransaction();
        Task SaveChangesAsync();

        IUserRepository UserRepository { get; }
        IUserAnswerRepository UserAnswerRepository { get; }
        ITeamRepository TeamRepository { get; }
        ISessionUserRepository SessionUserRepository { get; }
        ISessionRepository SessionRepository { get; }
        IQuizRepository QuizRepository { get; }
        IQuestionRepository QuestionRepository { get; }
        IPlayerResponseRepository PlayerResponseRepository { get; }
        IAnswerRepository AnswerRepository { get; }
    }
}