using Microsoft.AspNetCore.Mvc;
using TICTACTOEAPI.DAL.Interfaces;
using TICTACTOEAPI.Models.DTO;

namespace TICTACTOEAPI.Controllers
{
    [ApiController]
    [Route("api/scoreboard")]
    public class ScoreboardController : ControllerBase
    {
        private readonly IScoreboardRepository _scoreRepo;

        public ScoreboardController(IScoreboardRepository scoreRepo)
        {
            _scoreRepo = scoreRepo;
        }

        [HttpGet]
        public async Task<ActionResult<ScoreboardDto>> GetScoreboard()
        {
            var sb = await _scoreRepo.GetScoreboardAsync();
            return Ok(new ScoreboardDto
            {
                XWins = sb.XWins,
                OWins = sb.OWins,
                Draws = sb.Draws
            });
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetScoreboard()
        {
            await _scoreRepo.ResetScoreboardAsync();
            return NoContent();
        }
    }
}