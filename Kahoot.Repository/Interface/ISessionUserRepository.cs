using Kahoot.Repository.Base;
using Kahoot.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Repository.Interface
{
    public interface ISessionUserRepository : IGenericRepository<GameSessionUser>
    {
    }
}
