using Kahoot.Repository.Base;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using System;

namespace Kahoot.Repository.Repositories
{
    public class SessionUserRepository : GenericRepository<GameSessionUser>, ISessionUserRepository
    {
        public SessionUserRepository(KahootContext context) : base(context)
        {
        }
    }

}