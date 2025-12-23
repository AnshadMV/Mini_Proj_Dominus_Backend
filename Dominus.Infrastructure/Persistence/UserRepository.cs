using Dominus.Application.Interfaces.IRepository;
using Dominus.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Infrastructure.Persistence
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task BlockUnblockUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null && !user.IsDeleted)
            {
                user.IsBlocked = !user.IsBlocked;
                user.ModifiedOn = DateTime.UtcNow;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SoftDeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null && !user.IsDeleted)
            {
                user.IsDeleted = true;
                user.DeletedOn = DateTime.UtcNow;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
