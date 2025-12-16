using Dominus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.Interfaces
{
    public interface IColorRepository : IGenericRepository<Color>
    {
        Task<Color?> GetByNameAsync(string name);
        Task<List<Color>> GetByIdsAsync(List<int> ids);
    }

}
