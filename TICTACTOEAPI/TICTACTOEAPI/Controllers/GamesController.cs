using Microsoft.AspNetCore.Mvc;
using TICTACTOEAPI.BAL.Interfaces;
using TICTACTOEAPI.Models.DTO;

namespace TICTACTOEAPI.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost]
        public async Task<ActionResult<GameStateDto>> CreateGame([FromBody] CreateGameRequest request)
        {
            var state = await _gameService.CreateGameAsync(request.Mode);
            return CreatedAtAction(nameof(GetGame), new { id = state.GameId }, state);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GameStateDto>> GetGame(Guid id)
        {
            var state = await _gameService.GetGameAsync(id);
            if (state is null) return NotFound();
            return Ok(state);
        }

        [HttpPost("{id}/moves")]
        public async Task<ActionResult<GameStateDto>> MakeMove(Guid id, [FromBody] MakeMoveRequest request)
        {
            var (success, error, state) = await _gameService.MakeMoveAsync(id, request.Player, request.CellIndex);
            if (!success) return BadRequest(new { error });
            return Ok(state);
        }

        [HttpPost("{id}/undo")]
        public async Task<ActionResult<GameStateDto>> Undo(Guid id)
        {
            var (success, error, state) = await _gameService.UndoAsync(id);
            if (!success) return BadRequest(new { error });
            return Ok(state);
        }

        [HttpPost("{id}/reset")]
        public async Task<ActionResult<GameStateDto>> Reset(Guid id)
        {
            var state = await _gameService.ResetGameAsync(id);
            if (state is null) return NotFound();
            return Ok(state);
        }
    }
}