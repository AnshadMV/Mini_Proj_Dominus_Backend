using Dominus.Domain.Entities;

namespace Dominus.Domain.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<List<Order>> GetByUserIdAsync(string userId);
        Task<Order?> GetByIdWithItemsAsync(int orderId);
    }
}
