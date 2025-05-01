using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kahoot.Common.BusinessResult;

namespace Kahoot.Service.Interface
{
    public interface IDashboardService
    {
        Task<IBusinessResult> Revenue();
        Task<IBusinessResult> Transaction(int pageIndex, int pageSize, string? search);

    }
}
