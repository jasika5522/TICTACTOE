using PortfolioService.Models.Entity;

namespace PortfolioService.DAL.Interfaces;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<RegisterUserDbResult> RegisterUserAsync(string fullName, string email, string mobileNumber, string passwordHash);
}
