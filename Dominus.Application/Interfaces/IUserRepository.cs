using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task BlockUnblockUserAsync(int id);
        Task SoftDeleteUserAsync(int id);
    }
}
