using ECommerce.Data;
using ECommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ========== AUTH ==========

        public IActionResult Login()
        {
           
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var row = _context.Admins.FirstOrDefault(a => a.Email == email);
            if (row != null && row.Password == password)
            {
                HttpContext.Session.SetString("admin_session", row.Id.ToString());
                return RedirectToAction("Index");
            }
            ViewBag.Message = "Invalid email or password";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("admin_session");
            return RedirectToAction(nameof(Login));
        }

        // ========== PROFILE ==========

        public IActionResult Profile()
        {
            if (HttpContext.Session.GetString("admin_session") == null)
                return RedirectToAction(nameof(Login));

            int id = int.Parse(HttpContext.Session.GetString("admin_session"));
            var admin = _context.Admins.FirstOrDefault(a => a.Id == id);
            return View(admin);
        }

        [HttpPost]
        public IActionResult Profile(Admin admin)
        {
            if (HttpContext.Session.GetString("admin_session") == null)
                return RedirectToAction(nameof(Login));

            _context.Admins.Update(admin);
            _context.SaveChanges();
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        public IActionResult ChangeProfileImage(IFormFile Image, int adminId)
        {
            if (HttpContext.Session.GetString("admin_session") == null)
                return RedirectToAction(nameof(Login));

            if (Image != null && Image.Length > 0)
            {
                string folderPath = Path.Combine(_env.WebRootPath, "AdminImage");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = Guid.NewGuid() + Path.GetExtension(Image.FileName);
                string imagePath = Path.Combine(folderPath, fileName);

                using (FileStream fs = new FileStream(imagePath, FileMode.Create))
                    Image.CopyTo(fs);

                var admin = _context.Admins.Find(adminId);
                if (admin != null)
                {
                    admin.Image = fileName;
                    _context.Admins.Update(admin);
                    _context.SaveChanges();
                    TempData["Success"] = "Profile image updated!";
                }
            }
            return RedirectToAction(nameof(Profile));
        }

        // ========== CUSTOMERS ==========

     
    }
}