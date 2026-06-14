namespace TICTACTOEAPI.Models.DTO
{
    public class MakeMoveRequest
    {
        public char Player { get; set; }
        public int CellIndex { get; set; } // 0-8
    }
}
