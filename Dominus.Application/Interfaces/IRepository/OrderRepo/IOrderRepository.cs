using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;

namespace Dominus.Application.Interfaces.IRepository.OrderRepo
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<List<Order>> GetByUserIdAsync(string userId);
        Task<Order?> GetByIdWithItemsAsync(int orderId);
    }
}
