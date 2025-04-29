using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kahoot.Common.BusinessResult;
using Kahoot.Common;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using Kahoot.Service.Interface;
using Kahoot.Service.Model.Response;
using Mapster;
using Kahoot.Service.Model.Request;

namespace Kahoot.Service.Services
{
    public class SystemConfigationService : ISystemConfigurationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SystemConfigationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IBusinessResult> GetSystemConfig(int pageIndex, int pageSize, string? search)
        {
            var list = await _unitOfWork.SystemConfigurationRepository.GetPagedAsync(
                        pageIndex,
                        pageSize,
                        x => string.IsNullOrEmpty(search) || x.Name.ToLower().Contains(search.ToLower())
                    );
            var response = list.Adapt<List<SystemConfigurationResponse>>();
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
        }

        public async Task<IBusinessResult> GetSystemConfigById(int configId)
        {
            var config = await _unitOfWork.SystemConfigurationRepository.GetByIdAsync(configId);
            if (config == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "config not found");
            }
            var response = config.Adapt<SystemConfigurationResponse>();
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
        }

        public async Task<IBusinessResult> CreateSystemConfig(SystemConfigurationRequest request)
        {
            await _unitOfWork.SystemConfigurationRepository.AddAsync(request.Adapt<SystemConfiguration>());
            await _unitOfWork.SaveChangesAsync();
            return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG);
        }

        public async Task<IBusinessResult> UpdateSystemConfig(int configId, SystemConfigurationRequest request)
        {
            var config = await _unitOfWork.SystemConfigurationRepository.GetByIdAsync(configId);
            if (config == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "config not found");
            }

            if (request.MinValue > request.MaxValue)
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "MinValue must be less than MaxValue");
            }
            config.UpdatedAt = DateTime.Now;
            request.Adapt(config);
            await _unitOfWork.SaveChangesAsync();
            return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_UPDATE_MSG);
        }

        public async Task<IBusinessResult> DeleteSystemConfig(int configId)
        {
            var config = await _unitOfWork.SystemConfigurationRepository.GetByIdAsync(configId);
            if (config == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "config not found");
            }
            await _unitOfWork.SystemConfigurationRepository.DeleteAsync(config);
            await _unitOfWork.SaveChangesAsync();
            return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG);
        }
    }
}
