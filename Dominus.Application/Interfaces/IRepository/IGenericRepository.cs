using Dominus.Domain.Entities;
using System.Linq.Expressions;

namespace Dominus.Application.Interfaces.IRepository
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        
        Task AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);

        Task DeleteAsync(int id);

        Task SaveChangesAsync();

        IQueryable<T> Query();

        Task<T?> GetByIdAsync(int id);


        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null
        );

        Task<T?> GetAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null
        );

    }
}
