namespace TICTACTOEAPI.Models.DTO
{
    public class GameStateDto
    {
        public Guid GameId { get; set; }
        public string Board { get; set; } = default!; // 9-char string
        public char CurrentPlayer { get; set; }
        public string Mode { get; set; } = default!;
        public string Status { get; set; } = default!;
        public char? Winner { get; set; }
        public List<int>? WinningCells { get; set; }
        public List<MoveDto> MoveHistory { get; set; } = new();
        public ScoreboardDto Scoreboard { get; set; } = new();
    }
}
