using Kahoot.Repository.Base;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using System;

namespace Kahoot.Repository.Repositories
{
    public class PlayerRepository : GenericRepository<Player>, IPlayerRepository
    {
        public PlayerRepository(KahootContext context) : base(context)
        {
        }
    }

}