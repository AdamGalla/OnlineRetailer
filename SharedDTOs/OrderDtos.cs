namespace SharedDTOs;

public class OrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime? Date { get; set; }
    public OrderStatusDto Status { get; set; }
    public ICollection<OrderLineDto> OrderLine { get; set; }
}

public class OrderLineDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int Quantity { get; set; }
    public int ProductId { get; set; }
}

public enum OrderStatusDto
{
    Cancelled,
    Completed,
    Shipped,
    Paid
}
