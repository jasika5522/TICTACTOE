using Npgsql;
using System.Data;

namespace PortfolioService.DAL.DbConnection;

public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PortfolioDb")
            ?? throw new InvalidOperationException("Connection string 'PortfolioDb' was not found.");
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
