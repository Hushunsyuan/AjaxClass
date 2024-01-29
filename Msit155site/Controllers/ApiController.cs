using Microsoft.AspNetCore.Mvc;
using Msit155site.Models;

namespace Msit155site.Controllers
{
    public class ApiController : Controller
    {
        private readonly MyDBContext _context;
        public ApiController(MyDBContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Cities()
        {
            var Cities = _context.Addresses.Select(p => p.City).Distinct();
            return Json(Cities);
        }
    }
}
