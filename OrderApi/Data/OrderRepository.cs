using Microsoft.EntityFrameworkCore;
using OrderApi.Models;

namespace OrderApi.Data;

public class OrderRepository : IRepository<Order>
{
    private readonly OrderApiContext db;

    public OrderRepository(OrderApiContext context)
    {
        db = context;
    }

    public IEnumerable<Order> GetAllByCustomerId(int id)
    {
        return db.Orders.Include(o => o.OrderLine).Where(x => x.CustomerId == id).ToList();
    }

    Order IRepository<Order>.Add(Order entity)
    {
        if (entity.Date == null)
            entity.Date = DateTime.Now;
        
        var newOrder = db.Orders.Add(entity).Entity;
        db.SaveChanges();
        return newOrder;
    }

    void IRepository<Order>.Edit(Order entity)
    {
        db.Entry(entity).State = EntityState.Modified;
        db.SaveChanges();
    }

    Order IRepository<Order>.Get(int id)
    {
        var order = db.Orders.Include(o => o.OrderLine).FirstOrDefault(o => o.Id == id);
        db.Entry(order).Reload();
        return order;

    }

    IEnumerable<Order> IRepository<Order>.GetAll()
    {
        return db.Orders.Include(o => o.OrderLine).ToList();
    }

    void IRepository<Order>.Remove(int id)
    {
        var order = db.Orders.FirstOrDefault(p => p.Id == id);
        db.Orders.Remove(order);
        db.SaveChanges();
    }
}
