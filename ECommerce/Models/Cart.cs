using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int CustomerId { get; set; }

        public int ProductQuantity { get; set; }

        public int CartStatus { get; set; }

        [ForeignKey("ProductId")]
        public Product? products { get; set; }
        [ForeignKey("CustomerId")]
        public Customer? customers { get; set; }
    }
}