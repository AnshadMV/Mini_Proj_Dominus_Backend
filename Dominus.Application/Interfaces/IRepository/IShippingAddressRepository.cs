using Dominus.Domain.Entities;

namespace Dominus.Application.Interfaces.IRepository
{
    public interface IShippingAddressRepository
    {
        Task AddAsync(ShippingAddress address);
        Task<IEnumerable<ShippingAddress>> GetByUserIdAsync(int userId);
        Task<ShippingAddress?> GetByIdAsync(int id);
        Task SoftDeleteAsync(ShippingAddress address);
    }
}
