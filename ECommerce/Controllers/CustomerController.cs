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
            if (isLogin == null)
                return RedirectToAction(nameof(CustomerLogin));

            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                TempData["message"] = "Product Not Found!";
                return RedirectToAction(nameof(FetchAllProduct));
            }

            int customerId = int.Parse(isLogin);

          
            var existingCart = _context.Carts.FirstOrDefault(c =>
                c.ProductId == id &&
                c.CustomerId == customerId &&
                c.CartStatus == 0);

            if (existingCart != null)
            {
              
                existingCart.ProductQuantity += 1;
                _context.Carts.Update(existingCart);
            }
            else
            {
               
                var cart = new Cart
                {
                    ProductId = id,
                    CustomerId = customerId,
                    ProductQuantity = 1,
                    CartStatus = 0
                };
                _context.Carts.Add(cart);
            }

            _context.SaveChanges();
            TempData["message"] = "✅ Product Successfully Added in Cart";
            return RedirectToAction(nameof(FetchAllProduct));
        }
        public IActionResult IncreaseQuantity(int id)
        {
            var cart = _context.Carts.Find(id);
            if (cart != null)
            {
                cart.ProductQuantity += 1;
                _context.Carts.Update(cart);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(FetchCart));
        }

        public IActionResult DecreaseQuantity(int id)
        {
            var cart = _context.Carts.Find(id);
            if (cart != null)
            {
                if (cart.ProductQuantity > 1)
                {
                    cart.ProductQuantity -= 1;
                    _context.Carts.Update(cart);
                    _context.SaveChanges();
                }
                else
                {
                    // ✅ Quantity 1 হলে cart থেকে remove
                    _context.Carts.Remove(cart);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction(nameof(FetchCart));
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



        // ✅ MyOrders - শুধু orders দেখাবে
        public IActionResult MyOrders()
        {
            string isLogin = HttpContext.Session.GetString("customerSession");
            if (isLogin == null)
                return RedirectToAction(nameof(CustomerLogin));

            int customerId = int.Parse(isLogin);

            var orders = _context.Orders
                .Include(o => o.carts)
                    .ThenInclude(c => c.products)
                .Include(o => o.carts)
                    .ThenInclude(c => c.customers)
                .Where(o => o.carts!.CustomerId == customerId)
                .ToList();

            List<Category> category = _context.Categories.ToList();
            ViewData["category"] = category;

            return View(orders);
        }

        
        public IActionResult PlaceOrder(int id)
        {
            string isLogin = HttpContext.Session.GetString("customerSession");
            if (isLogin == null)
                return RedirectToAction(nameof(CustomerLogin));

            var cart = _context.Carts.Find(id);
            if (cart == null)
            {
                TempData["message"] = "❌ Cart পাওয়া যায়নি!";
                return RedirectToAction(nameof(FetchCart));
            }

            // Duplicate check
            var existingOrder = _context.Orders.FirstOrDefault(o => o.CartId == id);
            if (existingOrder != null)
            {
                TempData["message"] = "⚠️ এই product এর order আগেই দেওয়া হয়েছে!";
                return RedirectToAction(nameof(FetchCart));
            }

            // Cart status update
            cart.CartStatus = 1;
            _context.Carts.Update(cart);

            // Order insert
            var order = new Order
            {
                CartId = id,
                Status = 0
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            TempData["message"] = "✅ Order সফলভাবে দেওয়া হয়েছে!";
            return RedirectToAction(nameof(FetchCart));
        }

    }
}
