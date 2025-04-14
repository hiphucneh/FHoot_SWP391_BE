using Kahoot.Repository.Base;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using System;

namespace Kahoot.Repository.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(KahootContext context) : base(context)
        {
        }
    }

}