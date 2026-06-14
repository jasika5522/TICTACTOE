namespace TICTACTOEAPI.Models.Entity
{
    public class ScoreboardEntity
    {
        public short Id { get; set; }
        public int XWins { get; set; }
        public int OWins { get; set; }
        public int Draws { get; set; }
    }
}
