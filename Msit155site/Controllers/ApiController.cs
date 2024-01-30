using Microsoft.AspNetCore.Mvc;
using Msit155site.Models;
using Msit155site.Models.DTO;
using System.Text;

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
            Thread.Sleep(50000);
            //int x = 10;
            //int y = 0;
            //int z = x / y;
            return Content("<H1>Content 中文</H1>","text/plain",Encoding.UTF8);
        }
        //public IActionResult Register(string name, int age = 28)
        public IActionResult Register(UserDTO _user)
        {
            if (string.IsNullOrEmpty(_user.Name))
            {
                _user.Name = "guest";
            }
            return Content($"Hello {_user.Name},{_user.Age}歲,電子郵件是{_user.Email}","text/plan", Encoding.UTF8);
        }
      [HttpPost]
        public IActionResult Cities()
        {
            var Cities = _context.Addresses.Select(p => p.City).Distinct();
            return Json(Cities);
        }
        public IActionResult Avatar(int id = 1)
        {
            Member? member = _context.Members.Find(id);
            if(member != null)
            {
                byte[] img = member.FileData;
                if (img != null)
                {
                    return File(img, "image/jpeg");
                }
            }
            return NotFound();
        }
    }
}
