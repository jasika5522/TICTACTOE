using TICTACTOEAPI.Models.Entity;

namespace TICTACTOEAPI.DAL.Interfaces
{
    public interface IGameRepository
    {
        Task<GameEntity?> GetGameAsync(Guid id);
        Task<Guid> CreateGameAsync(GameEntity game);
        Task UpdateGameAsync(GameEntity game);
        Task<List<MoveEntity>> GetMovesAsync(Guid gameId);
        Task AddMoveAsync(MoveEntity move);
        Task<int> GetNextMoveNumberAsync(Guid gameId);
        Task RemoveLastMovesAsync(Guid gameId, int count);
    }
}
