using System.Collections.Generic;

namespace ECommerce.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalIncome { get; set; }
        public List<string> OrderStatusLabels { get; set; } = new List<string>();
        public List<int> OrderStatusData { get; set; } = new List<int>();
    }
}