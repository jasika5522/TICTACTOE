using TICTACTOEAPI.Models.DTO;

namespace TICTACTOEAPI.BAL.Interfaces
{
    public interface IGameService
    {
        Task<GameStateDto> CreateGameAsync(string mode);
        Task<GameStateDto?> GetGameAsync(Guid id);
        Task<(bool success, string? error, GameStateDto? state)> MakeMoveAsync(Guid id, char player, int cellIndex);
        Task<(bool success, string? error, GameStateDto? state)> UndoAsync(Guid id);
        Task<GameStateDto?> ResetGameAsync(Guid id);
    }
}
