using Dominus.Domain.Entities;

namespace Dominus.Application.Interfaces.IRepository
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart?> GetByUserIdAsync(string userId);
    }
}
