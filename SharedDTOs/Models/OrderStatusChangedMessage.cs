namespace SharedDTOs.Models;

public class OrderStatusChangedMessage
{
    public int? CustomerId { get; set; }
    public IList<OrderLineDto>? OrderLines { get; set; }
}
