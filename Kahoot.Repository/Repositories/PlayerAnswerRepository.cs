using Kahoot.Repository.Base;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using System;

namespace Kahoot.Repository.Repositories
{
    public class PlayerAnswerRepository : GenericRepository<PlayerAnswer>, IPlayerAnswerRepository
    {
        public PlayerAnswerRepository(KahootContext context) : base(context)
        {
        }
    }

}