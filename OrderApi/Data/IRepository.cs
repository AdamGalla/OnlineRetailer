using System.Collections.Generic;

namespace OrderApi.Data
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAllByCustomerId(int id);
        T Get(int id);
        T Add(T entity);
        void Edit(T entity);
        void Remove(int id);
    }
}
