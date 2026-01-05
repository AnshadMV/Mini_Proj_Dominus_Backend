using Dominus.Application.Interfaces.IRepository;
using Dominus.Domain.Entities;

namespace Dominus.Domain.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<List<Order>> GetByUserIdAsync(string userId);
        Task<List<Order>> GetByUserAndProductAsync(
    string userId,
    int productId
);

        Task<Order?> GetByIdWithItemsAsync(int orderId);

        Task<List<Order>> GetAllWithItemsAsync();
        Task<Product?> GetByIdTrackedAsync(int id);

    }
}
