using Microsoft.AspNetCore.Identity;
using PortfolioService.BAL.Interfaces;
using PortfolioService.DAL.Interfaces;
using PortfolioService.Models.DTO;
using PortfolioService.Models.Entity;

namespace PortfolioService.BAL.Implementations;

public class UserRegistrationService : IUserRegistrationService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher<UserEntity> _passwordHasher = new();

    public UserRegistrationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<(bool IsSuccess, bool IsDuplicate, string Message, Dictionary<string, string[]>? Errors)> RegisterAsync(
        UserRegistrationRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.EmailExistsAsync(normalizedEmail))
        {
            return (false, true, "Email is already registered.", null);
        }

        var passwordHash = _passwordHasher.HashPassword(new UserEntity(), request.Password);

        var dbResult = await _userRepository.RegisterUserAsync(
            request.FullName,
            normalizedEmail,
            request.MobileNumber,
            passwordHash);

        if (!dbResult.Success && dbResult.Message.Contains("already registered", StringComparison.OrdinalIgnoreCase))
        {
            return (false, true, dbResult.Message, null);
        }

        if (!dbResult.Success)
        {
            return (false, false, dbResult.Message, null);
        }

        return (true, false, dbResult.Message, null);
    }
}
