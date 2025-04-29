using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;

namespace Kahoot.Repository.Repositories
{
    public class SystemConfigurationRepository : GenericRepository<SystemConfiguration>, ISystemConfigurationRepository
    {
        public SystemConfigurationRepository(KahootContext context) : base(context)
        {
        }
    }
}
