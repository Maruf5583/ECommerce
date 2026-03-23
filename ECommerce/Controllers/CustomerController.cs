using ECommerce.Data;
using ECommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Controllers
{
    public class CustomerController : Controller
    {
        private AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public CustomerController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        public IActionResult Index()
        {
            List<Category> category = _context.Categories.ToList();
            ViewData["category"] = category;

            List<Product> product = _context.Products.ToList();
            ViewData["product"] = product;

            ViewBag.checkSession = HttpContext.Session.GetString("customerSession");
            return View();
        }


        public IActionResult CustomerLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CustomerLogin(Customer model)
        {
            var customer = _context.Customers
                .FirstOrDefault(c => c.Eamil == model.Eamil && c.Password == model.Password);

            if (customer != null)
            {
                HttpContext.Session.SetString("customerSession", customer.Id.ToString());
                return RedirectToAction("Index");
            }

            ViewBag.message = "Invalid email or password";
            return View();
        }
        public IActionResult CustomerRegistration()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CustomerRegistration(Customer customer, IFormFile Image)
        {
            if (Image != null && Image.Length > 0)
            {
                string folderPath = Path.Combine(_environment.WebRootPath, "CustomerImageRegister");

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
                return RedirectToAction(nameof(CustomerLogin));


            }
            return View();


        }
        public IActionResult CustomerLogout()
        {
            HttpContext.Session.Remove("customerSession");
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CustomerProfile()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("customerSession")))
            {
                return RedirectToAction(nameof(CustomerLogin));
            }
            else
            {
                var customerId = HttpContext.Session.GetString("customerSession");
                var row = _context.Customers.Where(a => a.Id == int.Parse(customerId)).ToList();
                List<Category> category = _context.Categories.ToList();
                ViewData["category"] = category;
                return View(row);
            }

        }

        public IActionResult UpdateCustomerProfile(Customer customer)
        {
            _context.Customers.Update(customer);
            _context.SaveChanges();
            return RedirectToAction(nameof(CustomerProfile));
        }
        public IActionResult UpdateCustomerImage(Customer customer, IFormFile Image)
        {
            if (Image != null && Image.Length > 0)
            {
                string folderPath = Path.Combine(_environment.WebRootPath, "CustomerImageRegister");

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
            }

            return RedirectToAction(nameof(CustomerProfile));
        }


        public IActionResult Feedback()
        {
            List<Category> category = _context.Categories.ToList();
            ViewData["category"] = category;
            return View();
        }
        [HttpPost]
        public IActionResult Feedback(Feedback feedback)
        {
            TempData["message"] = "Thanks for submitting feedback";
            _context.Feedbacks.Add(feedback);
            _context.SaveChanges();
            return RedirectToAction(nameof(Feedback));
        }



        //all product show

        public IActionResult FetchAllProduct()
        {
            List<Category> category = _context.Categories.ToList();
            ViewData["category"] = category;

            List<Product> product = _context.Products.ToList();
            ViewData["product"] = product;
            return View();
        }

        public IActionResult ProductDetails(int id)
        {
            List<Category> category = _context.Categories.ToList();
            ViewData["category"] = category;

            var products = _context.Products.Where(p => p.Id == id).ToList();
            return View(products);
        }


        ///cart

        public IActionResult AddToCart(int id)
        {
            string isLogin = HttpContext.Session.GetString("customerSession");
            if (isLogin != null)
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == id);
                if (product == null)
                {
                    TempData["message"] = "Product Not Found!";
                    return RedirectToAction(nameof(FetchAllProduct));
                }

                Cart cart = new Cart();
                cart.ProductId = product.Id;
                cart.CustomerId = int.Parse(isLogin);
                cart.ProductQuantity = 1;
                cart.CartStatus = 0;

                _context.Carts.Add(cart);
                _context.SaveChanges();

                TempData["message"] = "✅ Product Successfully Added in Cart";
                return RedirectToAction(nameof(FetchAllProduct));
            }
            return RedirectToAction(nameof(CustomerLogin));
        }

        public IActionResult FetchCart()
        {
            List<Category> category = _context.Categories.ToList();
            ViewData["category"] = category;
            string customerId = HttpContext.Session.GetString("customerSession");
            if (customerId != null && int.TryParse(customerId, out int custId))
            {
                var cart = _context.Carts
                    .Where(c => c.CustomerId == custId)
                    .Include(c => c.products)
                    .Include(c => c.customers)
                    .ToList();

                return View(cart);
            }
            else
            {
                return RedirectToAction(nameof(CustomerLogin));
            }


        }

        public IActionResult RemoveProduct(int id)
        {
            var product = _context.Carts.Find(id);
            _context.Carts.Remove(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(FetchCart));
        }

    }
}
