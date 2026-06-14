namespace TICTACTOEAPI.Models.DTO
{
    public class MoveDto
    {
        public int MoveNumber { get; set; }
        public char Player { get; set; }
        public int CellIndex { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }

}
