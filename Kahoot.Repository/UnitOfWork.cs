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
        private IUserAnswerRepository _userAnswerRepository;
        private ITeamRepository _teamRepository;
        private ISessionUserRepository _sessionUserRepository;
        private ISessionRepository _sessionRepository;
        private IQuizRepository _quizRepository;
        private IQuestionRepository _questionRepository;
        private IPlayerResponseRepository _playerResponseRepository;
        private IAnswerRepository _answerRepository;

        public UnitOfWork(KahootContext context)
        {
            _context = context;
        }

        public IUserRepository UserRepository => _userRepository ??= new UserRepository(_context);

        public IUserAnswerRepository UserAnswerRepository => _userAnswerRepository ??= new UserAnswerRepository(_context);

        public ITeamRepository TeamRepository => _teamRepository ??= new TeamRepository(_context);

        public ISessionUserRepository SessionUserRepository => _sessionUserRepository ??= new SessionUserRepository(_context);

        public ISessionRepository SessionRepository => _sessionRepository ??= new SessionRepository(_context);

        public IQuizRepository QuizRepository => _quizRepository ??= new QuizRepository(_context);

        public IQuestionRepository QuestionRepository => _questionRepository ??= new QuestionRepository(_context);

        public IPlayerResponseRepository PlayerResponseRepository => _playerResponseRepository ??= new PlayerResponseRepository(_context);

        public IAnswerRepository AnswerRepository => _answerRepository ??= new AnswerRepository(_context);

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
