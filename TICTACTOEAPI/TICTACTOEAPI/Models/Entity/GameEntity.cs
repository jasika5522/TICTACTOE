namespace TICTACTOEAPI.Models.Entity
{
    public class GameEntity
    {
        public Guid Id { get; set; }
        public string Mode { get; set; } = default!;
        public string Board { get; set; } = default!;
        public char CurrentPlayer { get; set; }
        public string Status { get; set; } = default!;
        public char? Winner { get; set; }
        public short[]? WinningCells { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
