using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kahoot.Service.Interface;
using Kahoot.Service.ModelDTOs.Request;

namespace Kahoot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PackageController : ControllerBase
    {
        private readonly IPackageService _packageService;

        public PackageController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPackage()
        {
            var result = await _packageService.GetPackage();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{packageId}")]
        [Authorize]
        public async Task<IActionResult> GetPackageById(int packageId)
        {
            var result = await _packageService.GetPackageById(packageId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user-package")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserPackages(int pageIndex, int pageSize, string? status, string? search)
        {
            var result = await _packageService.GetUserPackage(pageIndex, pageSize, status, search);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] PackageRequest request)
        {
            var result = await _packageService.CreatePackage(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{packageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int packageId, [FromBody] PackageRequest request)
        {
            var result = await _packageService.UpdatePackage(packageId, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{packageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int packageId)
        {
            var result = await _packageService.DeletePackage(packageId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("payment")]
        [Authorize]
        public async Task<IActionResult> PayforPackage(string cancelUrl, string returnUrl, int packageId)
        {
            var result = await _packageService.PayforPackage(cancelUrl, returnUrl, packageId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("payos-callback")]
        public async Task<IActionResult> PAYOSCallback(string status)
        {
            var result = await _packageService.PAYOSCallback(status);
            return StatusCode(result.StatusCode, result);
        }
    }
}