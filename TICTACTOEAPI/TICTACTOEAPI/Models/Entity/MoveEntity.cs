namespace TICTACTOEAPI.Models.Entity
{
    public class MoveEntity
    {
        public long Id { get; set; }
        public Guid GameId { get; set; }
        public int MoveNumber { get; set; }
        public char Player { get; set; }
        public short CellIndex { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
