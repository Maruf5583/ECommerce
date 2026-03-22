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

        public IActionResult FetchCustomer()
        {
            return View(_context.Customers.ToList());
        }
        public IActionResult CustomerDetails(int id)
        {
            var CustomerId = _context.Customers.FirstOrDefault(c => c.Id == id);
            return View(CustomerId);
        }
        public IActionResult CustomerUpdate(int id)
        {
            var CustomerId = _context.Customers.Find(id);
            return View(CustomerId);    
        }
        [HttpPost]
        public IActionResult CustomerUpdate(Customer customer, IFormFile Image)
        {
            if (Image != null && Image.Length > 0)
            {
                string folderPath = Path.Combine(_env.WebRootPath, "CustomerImage");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = Path.GetFileName(Image.FileName);
                string imagePath = Path.Combine(folderPath, fileName);

                using (FileStream fs = new FileStream(imagePath, FileMode.Create))
                {
                    Image.CopyTo(fs);
                }

                customer.Image = fileName;
                _context.Customers.Update(customer);
                _context.SaveChanges();
                return RedirectToAction(nameof(FetchCustomer));


            }
            return View();


        }


        public IActionResult CustomerDelete(int id)
        {
            var Customer = _context.Customers.Find(id);
            return View(Customer);
        }


        [HttpPost]
        [ActionName("CustomerDelete")]
        public IActionResult CustomerDeleteConfirmed(int id)
        {
            var Customer = _context.Customers.Find(id);
            _context.Customers.Remove(Customer);
            _context.SaveChanges();
            return RedirectToAction(nameof(FetchCustomer));
        }


        //Category
        public IActionResult FetchCategory()
        {
            return View(_context.Categories.ToList());
        }
        public IActionResult AddCategory()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddCategory(Category cat)
        {
            _context.Categories.Add(cat);
            _context.SaveChanges();
            return RedirectToAction(nameof(FetchCategory));
        }

        public IActionResult UpdateCategory(int id)
        {
            var category = _context.Categories.Find(id);
            return View(category);
        }
        [HttpPost]
        public IActionResult UpdateCategory(Category cat)
        {
            _context.Categories.Update(cat);
            _context.SaveChanges();
            return RedirectToAction(nameof(FetchCategory));
        }

        public IActionResult DeleteCategoryConfirmed(int id)
        {

            return View(_context.Categories.FirstOrDefault(c => c.Id == id));
        }



        public IActionResult DeleteCategory(int id)
        {
            var Category = _context.Categories.Find(id);
            _context.Categories.Remove(Category);
            _context.SaveChanges();
            return RedirectToAction(nameof(FetchCategory));
        }

    }
}