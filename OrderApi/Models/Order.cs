using System;
namespace OrderApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime? Date { get; set; }
        public string Status { get; set; }
        public ICollection<OrderLine> OrderLine { get; set; }
        
    }
}
