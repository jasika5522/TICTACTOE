using Dapper;
using TICTACTOEAPI.DAL.DbConnection;
using TICTACTOEAPI.DAL.Interfaces;
using TICTACTOEAPI.Models.Entity;

namespace TICTACTOEAPI.DAL.Implementations
{
    public class ScoreboardRepository : IScoreboardRepository
    {
        private readonly DapperContext _context;
        public ScoreboardRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<ScoreboardEntity> GetScoreboardAsync()
        {
            const string sql = "SELECT * FROM get_scoreboard()";

            using var connection = _context.CreateConnection();
            return await connection.QuerySingleAsync<ScoreboardEntity>(sql);
        }

        public async Task IncrementXWinsAsync()
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync("SELECT increment_x_wins()");
        }

        public async Task IncrementOWinsAsync()
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync("SELECT increment_o_wins()");
        }

        public async Task IncrementDrawsAsync()
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync("SELECT increment_draws()");
        }

        public async Task DecrementXWinsAsync()
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync("SELECT decrement_x_wins()");
        }

        public async Task DecrementOWinsAsync()
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync("SELECT decrement_o_wins()");
        }

        public async Task DecrementDrawsAsync()
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync("SELECT decrement_draws()");
        }

        public async Task ResetScoreboardAsync()
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync("SELECT reset_scoreboard()");
        }
    }
}