namespace TICTACTOEAPI.BAL.Interfaces
{
    public interface IGameLogicService
    {
        (bool hasWinner, char? winner, List<int>? winningCells) CheckWinner(string board);
        bool IsDraw(string board);
        bool IsValidMove(string board, int cellIndex);
        int GetComputerMove(string board);
    }
}
