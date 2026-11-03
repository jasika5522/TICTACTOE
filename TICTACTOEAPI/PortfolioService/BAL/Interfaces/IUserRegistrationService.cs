using PortfolioService.Models.DTO;

namespace PortfolioService.BAL.Interfaces;

public interface IUserRegistrationService
{
    Task<(bool IsSuccess, bool IsDuplicate, string Message, Dictionary<string, string[]>? Errors)> RegisterAsync(UserRegistrationRequest request);
}
