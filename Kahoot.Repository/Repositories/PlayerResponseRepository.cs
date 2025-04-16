using Kahoot.Repository.Base;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using System;

namespace Kahoot.Repository.Repositories
{
    public class PlayerResponseRepository : GenericRepository<PlayerResponse>, IPlayerResponseRepository
    {
        public PlayerResponseRepository(KahootContext context) : base(context)
        {
        }
    }

}