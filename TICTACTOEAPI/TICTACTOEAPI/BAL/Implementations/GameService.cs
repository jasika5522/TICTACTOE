using TICTACTOEAPI.BAL.Interfaces;
using TICTACTOEAPI.DAL.Interfaces;
using TICTACTOEAPI.Models.DTO;
using TICTACTOEAPI.Models.Entity;

namespace TICTACTOEAPI.BAL.Implementations
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepo;
        private readonly IScoreboardRepository _scoreRepo;
        private readonly IGameLogicService _logic;

        public GameService(IGameRepository gameRepo, IScoreboardRepository scoreRepo, IGameLogicService logic)
        {
            _gameRepo = gameRepo;
            _scoreRepo = scoreRepo;
            _logic = logic;
        }

        public async Task<GameStateDto> CreateGameAsync(string mode)
        {
            var game = new GameEntity
            {
                Id = Guid.NewGuid(),
                Mode = mode,
                Board = "_________",
                CurrentPlayer = 'X',
                Status = "InProgress"
            };

            await _gameRepo.CreateGameAsync(game);
            return await BuildGameStateAsync(game);
        }

        public async Task<GameStateDto?> GetGameAsync(Guid id)
        {
            var game = await _gameRepo.GetGameAsync(id);
            if (game is null) return null;
            return await BuildGameStateAsync(game);
        }

        public async Task<(bool success, string? error, GameStateDto? state)> MakeMoveAsync(Guid id, char player, int cellIndex)
        {
            var game = await _gameRepo.GetGameAsync(id);
            if (game is null) return (false, "Game not found", null);

            if (game.Status != "InProgress")
                return (false, "Game already completed", null);

            if (player != game.CurrentPlayer)
                return (false, "Not this player's turn", null);

            if (!_logic.IsValidMove(game.Board, cellIndex))
                return (false, "Invalid move", null);

            await ApplyMoveAsync(game, player, cellIndex);

            game = await _gameRepo.GetGameAsync(id);

            if (game!.Mode == "VsComputer" && game.Status == "InProgress" && game.CurrentPlayer == 'O')
            {
                var compMove = _logic.GetComputerMove(game.Board);
                if (compMove != -1)
                {
                    await ApplyMoveAsync(game, 'O', compMove);
                    game = await _gameRepo.GetGameAsync(id);
                }
            }

            var state = await BuildGameStateAsync(game!);
            return (true, null, state);
        }

        private async Task ApplyMoveAsync(GameEntity game, char player, int cellIndex)
        {
            var boardChars = game.Board.ToCharArray();
            boardChars[cellIndex] = player;
            game.Board = new string(boardChars);

            var moveNumber = await _gameRepo.GetNextMoveNumberAsync(game.Id);
            await _gameRepo.AddMoveAsync(new MoveEntity
            {
                GameId = game.Id,
                MoveNumber = moveNumber,
                Player = player,
                CellIndex = (short)cellIndex
            });

            var (hasWinner, winner, winningCells) = _logic.CheckWinner(game.Board);
            if (hasWinner)
            {
                game.Status = "Won";
                game.Winner = winner;
                game.WinningCells = winningCells!.Select(c => (short)c).ToArray();

                if (winner == 'X') await _scoreRepo.IncrementXWinsAsync();
                else await _scoreRepo.IncrementOWinsAsync();
            }
            else if (_logic.IsDraw(game.Board))
            {
                game.Status = "Draw";
                await _scoreRepo.IncrementDrawsAsync();
            }
            else
            {
                game.CurrentPlayer = player == 'X' ? 'O' : 'X';
            }

            await _gameRepo.UpdateGameAsync(game);
        }

        public async Task<(bool success, string? error, GameStateDto? state)> UndoAsync(Guid id)
        {
            var game = await _gameRepo.GetGameAsync(id);
            if (game is null) return (false, "Game not found", null);

            var moves = await _gameRepo.GetMovesAsync(id);
            if (moves.Count == 0) return (false, "No moves to undo", null);

            bool wasCompleted = game.Status != "InProgress";
            char? previousWinner = game.Winner;
            string previousStatus = game.Status;

            int movesToRemove = (game.Mode == "VsComputer" && moves.Count >= 2) ? 2 : 1;

            await _gameRepo.RemoveLastMovesAsync(id, movesToRemove);

            var remainingMoves = await _gameRepo.GetMovesAsync(id);
            var boardChars = "_________".ToCharArray();
            foreach (var m in remainingMoves)
                boardChars[m.CellIndex] = m.Player;
            var newBoard = new string(boardChars);

            var (hasWinner, winner, winningCells) = _logic.CheckWinner(newBoard);
            string newStatus;
            char? newWinner = null;
            short[]? newWinningCells = null;

            if (hasWinner)
            {
                newStatus = "Won";
                newWinner = winner;
                newWinningCells = winningCells!.Select(c => (short)c).ToArray();
            }
            else if (_logic.IsDraw(newBoard))
            {
                newStatus = "Draw";
            }
            else
            {
                newStatus = "InProgress";
            }

            // Adjust scoreboard if a completed game's result is being undone
            if (wasCompleted && newStatus == "InProgress")
            {
                if (previousWinner == 'X') await _scoreRepo.DecrementXWinsAsync();
                else if (previousWinner == 'O') await _scoreRepo.DecrementOWinsAsync();
                else if (previousStatus == "Draw") await _scoreRepo.DecrementDrawsAsync();
            }

            char nextPlayer = remainingMoves.Count == 0 ? 'X'
                : (remainingMoves[^1].Player == 'X' ? 'O' : 'X');

            game.Board = newBoard;
            game.Status = newStatus;
            game.Winner = newWinner;
            game.WinningCells = newWinningCells;
            game.CurrentPlayer = nextPlayer;

            await _gameRepo.UpdateGameAsync(game);

            var state = await BuildGameStateAsync(game);
            return (true, null, state);
        }

        public async Task<GameStateDto?> ResetGameAsync(Guid id)
        {
            var game = await _gameRepo.GetGameAsync(id);
            if (game is null) return null;

            await _gameRepo.RemoveLastMovesAsync(id, int.MaxValue);

            game.Board = "_________";
            game.CurrentPlayer = 'X';
            game.Status = "InProgress";
            game.Winner = null;
            game.WinningCells = null;

            await _gameRepo.UpdateGameAsync(game);
            return await BuildGameStateAsync(game);
        }

        private async Task<GameStateDto> BuildGameStateAsync(GameEntity game)
        {
            var moves = await _gameRepo.GetMovesAsync(game.Id);
            var scoreboard = await _scoreRepo.GetScoreboardAsync();

            return new GameStateDto
            {
                GameId = game.Id,
                Board = game.Board,
                CurrentPlayer = game.CurrentPlayer,
                Mode = game.Mode,
                Status = game.Status,
                Winner = game.Winner,
                WinningCells = game.WinningCells?.Select(c => (int)c).ToList(),
                MoveHistory = moves.Select(m => new MoveDto
                {
                    MoveNumber = m.MoveNumber,
                    Player = m.Player,
                    CellIndex = m.CellIndex,
                    Row = m.CellIndex / 3,
                    Column = m.CellIndex % 3
                }).ToList(),
                Scoreboard = new ScoreboardDto
                {
                    XWins = scoreboard.XWins,
                    OWins = scoreboard.OWins,
                    Draws = scoreboard.Draws
                }
            };
        }
    }
}