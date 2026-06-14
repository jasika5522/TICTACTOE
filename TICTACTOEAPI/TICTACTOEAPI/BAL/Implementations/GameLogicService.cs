using TICTACTOEAPI.BAL.Interfaces;

namespace TICTACTOEAPI.BAL.Implementations
{
    public class GameLogicService : IGameLogicService
    {
        private static readonly int[][] WinLines = new[]
        {
            new[]{0,1,2}, new[]{3,4,5}, new[]{6,7,8}, // rows
            new[]{0,3,6}, new[]{1,4,7}, new[]{2,5,8}, // columns
            new[]{0,4,8}, new[]{2,4,6}                // diagonals
        };

        public (bool hasWinner, char? winner, List<int>? winningCells) CheckWinner(string board)
        {
            foreach (var line in WinLines)
            {
                var a = board[line[0]];
                var b = board[line[1]];
                var c = board[line[2]];

                if (a != '_' && a == b && b == c)
                    return (true, a, line.ToList());
            }
            return (false, null, null);
        }

        public bool IsDraw(string board)
        {
            return !board.Contains('_');
        }

        public bool IsValidMove(string board, int cellIndex)
        {
            return cellIndex >= 0 && cellIndex <= 8 && board[cellIndex] == '_';
        }

        public int GetComputerMove(string board)
        {
            var move = FindWinningMove(board, 'O');
            if (move != -1) return move;

            move = FindWinningMove(board, 'X');
            if (move != -1) return move;

            if (board[4] == '_') return 4;

            foreach (var corner in new[] { 0, 2, 6, 8 })
                if (board[corner] == '_') return corner;

            for (int i = 0; i < 9; i++)
                if (board[i] == '_') return i;

            return -1;
        }

        private int FindWinningMove(string board, char player)
        {
            foreach (var line in WinLines)
            {
                var cells = line.Select(i => board[i]).ToArray();
                var emptyCount = cells.Count(c => c == '_');
                var playerCount = cells.Count(c => c == player);

                if (playerCount == 2 && emptyCount == 1)
                {
                    var emptyIndex = line[Array.FindIndex(cells, c => c == '_')];
                    return emptyIndex;
                }
            }
            return -1;
        }
    }
}