namespace OrderApi.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime? Date { get; set; }
    public OrderStatus Status { get; set; }
    public ICollection<OrderLine> OrderLine { get; set; }
}

public class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int Quantity { get; set; }
    public int ProductId { get; set; }
}

public enum OrderStatus
{
    Cancelled,
    Completed,
    Pending,
    Shipped,
    Paid
}
