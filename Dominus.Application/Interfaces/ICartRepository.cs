using Dominus.Domain.Entities;

namespace Dominus.Domain.Interfaces
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart?> GetByUserIdAsync(string userId);
    }
}
