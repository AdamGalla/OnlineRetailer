using OrderApi.Models;

namespace OrderApi.Data;

public interface IOrderRepository : IRepository<Order>
{
    IEnumerable<Order> GetAllByCustomerId(int customerId);
}
