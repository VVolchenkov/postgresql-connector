namespace PostgresqlConnector.DapperGenericRepository.Interfaces
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public interface IRepository<T>
    {
        Task<T> Get(int id);

        Task<ICollection<T>> GetAll();

        Task<ICollection<T>> GetByFilter(StringBuilder filter, object parameters = null);

        Task Update(T entity);

        Task Insert(T entity);

        Task<ICollection<T>> GetRaw(string filter, object parameters = null);
    }
}