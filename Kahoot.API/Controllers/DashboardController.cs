using Kahoot.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriDiet.Service.Enums;

namespace Kahoot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("revenue")]
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}")]
        public async Task<IActionResult> Revenue()
        {
            var result = await _dashboardService.Revenue();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("transaction")]
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}")]
        public async Task<IActionResult> Transaction(int pageIndex, int pageSize, string? search)
        {
            var result = await _dashboardService.Transaction(pageIndex, pageSize, search);
            return StatusCode(result.StatusCode, result);
        }
    }
}
