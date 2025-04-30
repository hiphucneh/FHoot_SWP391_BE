using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;

namespace Kahoot.Repository.Repositories
{
    public class PackageRepository : GenericRepository<Package>, IPackageRepository
    {
        public PackageRepository(KahootContext context) : base(context) { }
    }
}
