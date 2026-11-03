using Microsoft.AspNetCore.Mvc;
using PortfolioService.BAL.Interfaces;
using PortfolioService.Models.DTO;

namespace PortfolioService.Controllers;

[ApiController]
[Route("api/portfolio")]
public class PortfolioController : ControllerBase
{
    private readonly IUserRegistrationService _userRegistrationService;

    public PortfolioController(IUserRegistrationService userRegistrationService)
    {
        _userRegistrationService = userRegistrationService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Validation failed.",
                Errors = errors
            });
        }

        var (isSuccess, isDuplicate, message, errorsFromService) = await _userRegistrationService.RegisterAsync(request);

        if (isDuplicate)
        {
            return Conflict(new ApiResponse
            {
                Success = false,
                Message = message
            });
        }

        if (!isSuccess)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errorsFromService
            });
        }

        return StatusCode(StatusCodes.Status201Created, new ApiResponse
        {
            Success = true,
            Message = message
        });
    }
}
