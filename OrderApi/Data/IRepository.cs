using OrderApi.Models;

namespace OrderApi.Data;

public interface IRepository<T>
{
    IEnumerable<T> GetAll();
    IEnumerable<Order> GetAllByCustomerId(int customerId);
    T Get(int id);
    T Add(T entity);
    void Edit(T entity);
    void Remove(int id);
}
