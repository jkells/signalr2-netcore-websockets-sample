using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Controllers
{
    [Route("/home")]
    public class HomeController : Controller
    {        
        public IActionResult Index()
        {
            return View();
        }
    }
}
