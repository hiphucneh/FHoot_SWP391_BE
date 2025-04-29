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
        IPlayerAnswerRepository PlayerAnswerRepository { get; }
        ITeamRepository TeamRepository { get; }
        ISessionRepository SessionRepository { get; }
        IQuizRepository QuizRepository { get; }
        IQuestionRepository QuestionRepository { get; }
        IPlayerRepository PlayerRepository { get; }
        IAnswerRepository AnswerRepository { get; }
        IQuestionSessionRepository QuestionSessionRepository { get; }
        ISystemConfigurationRepository SystemConfigurationRepository { get; }
    }
}