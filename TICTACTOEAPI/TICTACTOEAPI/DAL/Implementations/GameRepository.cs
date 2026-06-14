using Dapper;
using TICTACTOEAPI.DAL.DbConnection;
using TICTACTOEAPI.DAL.Interfaces;
using TICTACTOEAPI.Models.Entity;

namespace TICTACTOEAPI.DAL.Implementations
{
    public class GameRepository : IGameRepository
    {
        private readonly DapperContext _dapperContext;
        public GameRepository(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<GameEntity?> GetGameAsync(Guid id)
        {
            const string sql = "SELECT * FROM get_game(@Id)";

            using var connection = _dapperContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<GameEntity>(sql, new { Id = id });
        }

        public async Task<Guid> CreateGameAsync(GameEntity game)
        {
            const string sql = "SELECT create_game(@Id, @Mode)";

            using var connection = _dapperContext.CreateConnection();
            return await connection.ExecuteScalarAsync<Guid>(sql, new { game.Id, game.Mode });
        }

        public async Task UpdateGameAsync(GameEntity game)
        {
            const string sql = @"
                SELECT update_game(@Id, @Board, @CurrentPlayer, @Status, @Winner, @WinningCells)";

            using var connection = _dapperContext.CreateConnection();
            await connection.ExecuteAsync(sql, game);
        }

        public async Task<List<MoveEntity>> GetMovesAsync(Guid gameId)
        {
            const string sql = "SELECT * FROM get_moves(@GameId)";

            using var connection = _dapperContext.CreateConnection();
            var result = await connection.QueryAsync<MoveEntity>(sql, new { GameId = gameId });
            return result.ToList();
        }

        public async Task AddMoveAsync(MoveEntity move)
        {
            const string sql = "SELECT add_move(@GameId, @MoveNumber, @Player, @CellIndex)";

            using var connection = _dapperContext.CreateConnection();
            await connection.ExecuteAsync(sql, move);
        }

        public async Task<int> GetNextMoveNumberAsync(Guid gameId)
        {
            const string sql = "SELECT get_next_move_number(@GameId)";

            using var connection = _dapperContext.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, new { GameId = gameId });
        }

        public async Task RemoveLastMovesAsync(Guid gameId, int count)
        {
            const string sql = "SELECT remove_last_moves(@GameId, @Count)";

            using var connection = _dapperContext.CreateConnection();
            await connection.ExecuteAsync(sql, new { GameId = gameId, Count = count });
        }
    }
}