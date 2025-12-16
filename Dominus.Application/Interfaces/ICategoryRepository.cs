using Dominus.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Dominus.Domain.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name);
        Task<Category?> GetWithProductsAsync(int id);
        Task<IEnumerable<Category>> GetAllActiveAsync();
    }
}