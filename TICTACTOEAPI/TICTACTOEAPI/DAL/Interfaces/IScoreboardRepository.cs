using TICTACTOEAPI.Models.Entity;

namespace TICTACTOEAPI.DAL.Interfaces
{
    public interface IScoreboardRepository
    {
        Task<ScoreboardEntity> GetScoreboardAsync();
        Task IncrementXWinsAsync();
        Task IncrementOWinsAsync();
        Task IncrementDrawsAsync();
        Task DecrementXWinsAsync();
        Task DecrementOWinsAsync();
        Task DecrementDrawsAsync();
        Task ResetScoreboardAsync();
    }
}
