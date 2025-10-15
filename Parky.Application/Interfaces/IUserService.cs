using Parky.Domain.Entities;

namespace Parky.Application.Interfaces
{
    public interface IUserService
    {
        Task<User?> ValidateUser(string username, string password);
    }
}
