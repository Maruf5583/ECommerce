using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; } 

        public string Name { get; set; }

        public string Price { get; set; }

        public string Description { get; set; }

        public string Image {  get; set; }

        public int catagoryId { get; set; }
    }
}
