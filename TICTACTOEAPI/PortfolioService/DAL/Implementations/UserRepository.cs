using Dapper;
using PortfolioService.DAL.DbConnection;
using PortfolioService.DAL.Interfaces;
using PortfolioService.Models.Entity;

namespace PortfolioService.DAL.Implementations;

public class UserRepository : IUserRepository
{
    private readonly DapperContext _dapperContext;

    public UserRepository(DapperContext dapperContext)
    {
        _dapperContext = dapperContext;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        const string sql = "SELECT email_exists(@Email)";

        using var connection = _dapperContext.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(sql, new { Email = email.Trim() });
    }

    public async Task<RegisterUserDbResult> RegisterUserAsync(
        string fullName,
        string email,
        string mobileNumber,
        string passwordHash)
    {
        const string sql = "SELECT * FROM register_user(@FullName, @Email, @MobileNumber, @PasswordHash)";

        using var connection = _dapperContext.CreateConnection();
        var result = await connection.QuerySingleAsync<RegisterUserDbResult>(sql, new
        {
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            MobileNumber = mobileNumber.Trim(),
            PasswordHash = passwordHash
        });

        return result;
    }
}
