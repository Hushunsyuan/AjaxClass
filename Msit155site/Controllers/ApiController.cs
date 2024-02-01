using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Msit155site.Models;
using Msit155site.Models.DTO;
using System.Security.Cryptography;
using System.Text;

namespace Msit155site.Controllers
{
    public class ApiController : Controller
    {
        private readonly MyDBContext _context;
        private readonly IWebHostEnvironment _environment;
        public ApiController(MyDBContext context,IWebHostEnvironment host)
        {
            _context = context;
            _environment = host;

		}

        
        public IActionResult Index()
        {
            Thread.Sleep(50000);
            //int x = 10;
            //int y = 0;
            //int z = x / y;
            return Content("<H1>Content 中文</H1>","text/plain",Encoding.UTF8);
        }
        [HttpPost]
        //public IActionResult Register(string name, int age = 28)
        public IActionResult Register(Member _user, IFormFile Photo)
        {
            if (string.IsNullOrEmpty(_user.Name))
            {
                _user.Name = "guest";
            }
            //string uploadPath = @"C:\Shared\AjaxWorkspace\MSIT155Site\wwwroot\uploads\a.jpg";
            string fileName = "empty.jpg";
            if (Photo != null)
            {
                fileName = Photo.FileName;
            }
            string uploadPath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
            using(var fileStream = new FileStream(uploadPath,FileMode.Create))
            {
                Photo?.CopyTo(fileStream);
            }
            //return Content($"Hello {_user.Name}, {_user.Age}歲了，電子郵件是{_user.Email}圖片格式{Photo?.FileName}-{Photo?.ContentType} -{Photo?.Length}","text/plan", Encoding.UTF8);
            //新增到資料庫
            _user.FileName = fileName;
            //轉成二進位 因DB是二進位
            byte[]? imgByte = null;
            using (var memoryStream = new MemoryStream())
            {
                Photo?.CopyTo(memoryStream);
                imgByte = memoryStream.ToArray();
            }
            _user.FileData = imgByte;

			//密碼加鹽          
			// 設定 PBKDF2 參數
			int iterations = 10000;
			int numBytesRequested = 32;
			//產生鹽
			byte[] salt = GenerateRandomSalt();
			// 使用 PBKDF2 加密
			byte[] hashedPassword = KeyDerivation.Pbkdf2(_user.Password, salt, KeyDerivationPrf.HMACSHA512, iterations, numBytesRequested);

			// salt 和 Password 可以被儲存為位元組陣列或轉換成十六進位字串
			_user.Salt = Convert.ToBase64String(salt);
			_user.Password = Convert.ToBase64String(hashedPassword);

			_context.Members.Add(_user);
            _context.SaveChanges();
            //return Content(uploadPath);
            //return Content($"Hello {_user.Name}, {_user.Age}歲了，電子郵件是{_user.Email}圖片格式{Photo?.FileName}-{Photo?.ContentType} -{Photo?.Length}","text/plan", Encoding.UTF8);
            return Content($"您好 {_user.Name}，{_user.Age}歲了，電子郵件是{_user.Email}，" +
                $"圖片格式({Photo?.FileName}-{Photo?.ContentType} -{Photo?.Length}) 圖片存取路徑為{uploadPath}", "text/plan", Encoding.UTF8);
        }
		// 產生鹽
		private static byte[] GenerateRandomSalt()
		{
			byte[] salt = new byte[16]; // 16位元組的加鹽
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(salt);
			}
			return salt;
		}

		public IActionResult CheckAccountAction(string name)
        {
            bool check = _context.Members.Any(p => p.Name == name);

			if (check)
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
		//景點資料
		[HttpPost]
		public IActionResult Spots([FromBody] SearchDTO _search)
		{
			//根據分類編號搜尋
			var spots =_search.CategoryId==0?_context.SpotImagesSpots:_context.SpotImagesSpots.Where(s=>s.CategoryId==_search.CategoryId);
			//根據關鍵字搜尋景點標題描述
			if (!string.IsNullOrEmpty(_search.Keyword))
            {
                spots=spots.Where(s=>s.SpotTitle.Contains(_search.Keyword)||s.SpotDescription.Contains(_search.Keyword));
            }
			//排序
			switch (_search.SortBy)
			{
                //依據景點標題排序
				case "spotTitle":
					spots = _search.SortType == "asc" ? spots.OrderBy(s => s.SpotTitle) : spots.OrderByDescending(s => s.SpotTitle);
					break;
				//依據景點CategoryId排序
				case "categoryId":
					spots = _search.SortType == "asc" ? spots.OrderBy(s => s.CategoryId) : spots.OrderByDescending(s => s.CategoryId);
					break;
				//預設依據spotId排序
				default:
					spots = _search.SortType == "asc" ? spots.OrderBy(s => s.SpotId) : spots.OrderByDescending(s => s.SpotId);
					break;
			}

			//總共有幾筆
			int totalCount = spots.Count();
			//一頁幾筆資料
			int pageSize = _search.PageSize ?? 9;
			//計算總共有幾頁
			int totalPages = (int)Math.Ceiling((decimal)totalCount / pageSize);
			//目前第幾頁
			int page = _search.Page ?? 1;
			//分頁
			spots = spots.Skip((page - 1) * pageSize).Take(pageSize);

			//return Json(spots);

			SpotsPagingDTO spotsPaging = new SpotsPagingDTO();
			spotsPaging.TotalPages = totalPages;
			spotsPaging.SpotsResult = spots.ToList();
			return Json(spotsPaging);

		}
        public IActionResult SpotTitle(string title)
        {
            var titles = _context.Spots.Where(s => s.SpotTitle.Contains(title))
                .Select(s => s.SpotTitle).Take(8);
            return Json(titles);
        }

    }
}
