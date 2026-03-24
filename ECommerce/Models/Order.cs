using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int Status { get; set; }
        [ForeignKey("CartId")]
        public Cart? carts { get; set; }
    }
}
