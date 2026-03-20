using ECommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {

        }

        public DbSet<Admin> Admins { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }
        
        
        public DbSet<Cart> Carts { get; set; }

        public DbSet<Feedback> Feedbacks { get; set; }

        public DbSet <Faqs> Faqs { get; set; }

     
    }
}
