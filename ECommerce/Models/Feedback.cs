using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Message { get; set; } 
    }
}
