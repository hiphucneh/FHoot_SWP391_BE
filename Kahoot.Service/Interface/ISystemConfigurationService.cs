using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kahoot.Common.BusinessResult;
using Kahoot.Service.Model.Request;

namespace Kahoot.Service.Interface
{
    public interface ISystemConfigurationService
    {
        Task<IBusinessResult> GetSystemConfig(int pageIndex, int pageSize, string? search);
        Task<IBusinessResult> GetSystemConfigById(int configId);
        Task<IBusinessResult> CreateSystemConfig(SystemConfigurationRequest request);
        Task<IBusinessResult> UpdateSystemConfig(int configId, SystemConfigurationRequest request);
        Task<IBusinessResult> DeleteSystemConfig(int configId);
    }
}
