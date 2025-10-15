using Microsoft.EntityFrameworkCore;
using Parky.Application.Interfaces;
using Parky.Domain.Entities;
using Parky.Infrastructure.Context;

namespace Parky.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ParkyDbContext _context;
        public UserService(ParkyDbContext context)
        {
            _context = context;
        }
        public async Task<User?> ValidateUser(string username, string password)
        {
            var user = await _context.Users.AsNoTracking()
                            .Where(t => t.Username == username && t.Password == password)
                            .SingleOrDefaultAsync();
            return user;
        }
    }
}
