using Microsoft.AspNetCore.Mvc;

namespace Kahoot.API.Controllers
{
    public class SessionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
