using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using Kahoot.Repository.Repositories;
using System;
using System.Threading.Tasks;

namespace Kahoot.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        protected readonly KahootContext _context;
        private IUserRepository _userRepository;
        private ITeamRepository _teamRepository;
        private ISessionRepository _sessionRepository;
        private IQuizRepository _quizRepository;
        private IQuestionRepository _questionRepository;
        private IPlayerRepository _playerRepository;
        private IAnswerRepository _answerRepository;
        private IPlayerAnswerRepository _playerAnswerRepository;
        private IQuestionSessionRepository _questionSessionRepository;
        private ISystemConfigurationRepository _systemConfigurationRepository;
        public UnitOfWork(KahootContext context)
        {
            _context = context;
        }

        public IUserRepository UserRepository => _userRepository ??= new UserRepository(_context);

        public ITeamRepository TeamRepository => _teamRepository ??= new TeamRepository(_context);

        public ISessionRepository SessionRepository => _sessionRepository ??= new SessionRepository(_context);

        public IQuizRepository QuizRepository => _quizRepository ??= new QuizRepository(_context);

        public IQuestionRepository QuestionRepository => _questionRepository ??= new QuestionRepository(_context);

        public IPlayerRepository PlayerRepository => _playerRepository ??= new PlayerRepository(_context);

        public IAnswerRepository AnswerRepository => _answerRepository ??= new AnswerRepository(_context);

        public IPlayerAnswerRepository PlayerAnswerRepository => _playerAnswerRepository ??= new PlayerAnswerRepository(_context);
        public IQuestionSessionRepository QuestionSessionRepository => _questionSessionRepository ??= new QuestionSessionRepository(_context);
        public ISystemConfigurationRepository SystemConfigurationRepository => _systemConfigurationRepository ??= new SystemConfigurationRepository(_context);

        public async Task BeginTransaction()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransaction()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransaction()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
