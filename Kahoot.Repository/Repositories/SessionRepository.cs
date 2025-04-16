using Kahoot.Repository.Base;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using System;

namespace Kahoot.Repository.Repositories
{
    public class SessionRepository : GenericRepository<QuizSession>, ISessionRepository
    {
        public SessionRepository(KahootContext context) : base(context)
        {
        }
    }

}