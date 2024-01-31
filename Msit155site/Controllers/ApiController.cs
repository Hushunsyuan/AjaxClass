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
            return Content($"Hello {_user.Name}, {_user.Age}歲了，電子郵件是{_user.Email}","text/plan", Encoding.UTF8);
        }
        public IActionResult CheckAccountAction(string name)
        {
            if (_context.Members.Count(p => p.Name == name) != 0)
            {
                return Content("帳號已存在");
            }
            else { return Content("帳號可使用"); }
        }
        //[HttpPost]範例留用
        //讀取城市
        public IActionResult Cities()
        {
            var Cities = _context.Addresses.Select(p => p.City).Distinct();
            return Json(Cities);
        }
        //根據城市名稱讀取鄉鎮區
        public IActionResult Districtes(string city)
        {
            var Districts = _context.Addresses.Where(a=>a.City==city).Select(d => d.SiteId).Distinct();
            return Json(Districts);
        }
        //根據鄉鎮區名稱讀取路名
        public IActionResult Roades(string district)
        {
            var Roades = _context.Addresses.Where(d => d.SiteId == district).Select(r => r.Road).Distinct();
            return Json(Roades);
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
