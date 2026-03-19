namespace ECommerce.Models
{
    public class Cart
    {
        public int Id { get; set; }

         public int ProductId { get; set; }

        public int CustomerId { get; set; } 

        public int ProductQuantity { get; set; }

        public int CartStatus { get; set; }
    }
}
