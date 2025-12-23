using Dominus.Application.DTOs.ProductDTOs;
using Dominus.Domain.Common;
using Dominus.Domain.DTOs.ProductDTOs;
using Dominus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Application.Interfaces.IRepository
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<Product?> GetProductWithDetailsAsync(int id);

    }
}
