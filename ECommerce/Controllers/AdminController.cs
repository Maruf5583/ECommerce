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
            // Dashboard statistics
            var totalCustomers = _context.Customers.Count();
            var totalOrders = _context.Orders.Count();

            // Calculate total income by summing product price * quantity for orders
            decimal totalIncome = _context.Orders
                .Include(o => o.carts)
                    .ThenInclude(c => c.products)
                .Select(o => (o.carts != null && o.carts.products != null)
                    ? o.carts.products.Price * o.carts.ProductQuantity
                    : 0)
                .Sum();

            // Group orders by status for charting
            var statusGroups = _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            var model = new AdminDashboardViewModel
            {
                TotalCustomers = totalCustomers,
                TotalOrders = totalOrders,
                TotalIncome = totalIncome,
                OrderStatusLabels = statusGroups.Select(s => "Status " + s.Status).ToList(),
                OrderStatusData = statusGroups.Select(s => s.Count).ToList()
            };

            return View(model);
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
        //product
        public IActionResult FetchProduct()
        {
            var product = _context.Products.ToList();
            return View(product);
        }

        public IActionResult AddProduct()
        {
            List<Category> categories = _context.Categories.ToList();
            ViewData["category"] = categories;
            return View();
        }
        [HttpPost]
        public IActionResult AddProduct(Product product, IFormFile Image)
        {
            if (Image != null && Image.Length > 0)
            {
                string folderPath = Path.Combine(_env.WebRootPath, "ProductImage");

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

                product.Image = fileName;
                _context.Products.Add(product);
                _context.SaveChanges();
                return RedirectToAction(nameof(FetchProduct));

            }
            return View();

        }

        public IActionResult ProductDetails(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        [HttpGet]
        public IActionResult DeleteProductConfirmed(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }


        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(FetchProduct));
        }

        public IActionResult ProductUpdate(int id)
        {
            List<Category> categories = _context.Categories.ToList();
            ViewData["category"] = categories;
            var product = _context.Products.Find(id);
            ViewBag.selectedCategoryId = product.CategoryId;
            return View(product);
        }
        [HttpPost]
        public IActionResult ProductUpdate(Product pro)
        {
            _context.Products.Update(pro);
            _context.SaveChanges();
            return RedirectToAction(nameof(FetchProduct));
        }


        public IActionResult ChangeProductImage(IFormFile Image, Product product)
        {
            if (Image != null && Image.Length > 0)
            {
                string folderPath = Path.Combine(_env.WebRootPath, "ProductImage");

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

                product.Image = fileName;
                _context.Products.Update(product);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(FetchProduct));
        }
        //feedback

        public IActionResult FetchFeedback()
        {
            var feedback = _context.Feedbacks.ToList();
            return View(feedback);
        }

        public IActionResult DeleteFeedbackConfirmed(int id)
        {
            var feedback = _context.Feedbacks.FirstOrDefault(f => f.Id == id);
            return View(feedback);
        }
        public IActionResult DeleteFeedback(int id)
        {
            var feedback = _context.Feedbacks.Find(id);
            _context.Feedbacks.Remove(feedback);
            _context.SaveChanges();
            return RedirectToAction(nameof(FetchProduct));
        }

        //Cart

        public IActionResult FetchCart()
        {
          
            var cart = _context.Carts
                        .Include(c => c.products)
                        .Include(c => c.customers)
                        .ToList();
            return View(cart);
        }

        public IActionResult DeleteCartConfirmed(int id)
        {
            var cart = _context.Carts
                        .Include(c => c.products)
                        .Include(c => c.customers)
                        .FirstOrDefault(f => f.Id == id);
            return View(cart);
        }

        public IActionResult DeleteCart(int id)
        {
            var cart = _context.Carts.Find(id);
            _context.Carts.Remove(cart);
            _context.SaveChanges();
            return RedirectToAction(nameof(FetchCart));
        }

        public IActionResult UpdateCart(int id)
        {
            var cart = _context.Carts
                        .Include(c => c.products)
                        .Include(c => c.customers)
                        .FirstOrDefault(c => c.Id == id);
            return View(cart);
        }

        [HttpPost]
        public IActionResult UpdateCart(int cartStatus, Cart cart)
        {
            cart.CartStatus = cartStatus;
            _context.Carts.Update(cart);
            _context.SaveChanges();
            return RedirectToAction(nameof(FetchCart));
        }


        //orders
        public IActionResult FetchOrders()
        {
            var orders = _context.Orders
                .Include(o => o.carts)
                    .ThenInclude(c => c.products)
                .Include(o => o.carts)
                    .ThenInclude(c => c.customers)
                .ToList();
            return View(orders);
        }

        // Order Details
        public IActionResult OrderDetails(int id)
        {
            var order = _context.Orders
                .Include(o => o.carts)
                    .ThenInclude(c => c.products)
                .Include(o => o.carts)
                    .ThenInclude(c => c.customers)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return RedirectToAction(nameof(FetchOrders));
            }
            return View(order);
        }

        // Delete Confirm Page
        public IActionResult DeleteOrderConfirmed(int id)
        {
            var order = _context.Orders
                .Include(o => o.carts)
                    .ThenInclude(c => c.products)
                .Include(o => o.carts)
                    .ThenInclude(c => c.customers)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return RedirectToAction(nameof(FetchOrders));
            }
            return View(order);
        }

        // Delete Action
        public IActionResult DeleteOrder(int id)
        {
            var order = _context.Orders.Find(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(FetchOrders));
        }


    }
}