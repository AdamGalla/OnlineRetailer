using OrderApi.Models;

namespace OrderApi.Data;

public interface IOrderRepository : IRepository<Order>
{
    IEnumerable<Order> GetByCustomer(int customerId);
}
