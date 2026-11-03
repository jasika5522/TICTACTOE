namespace PortfolioService.Models.Entity;

public class RegisterUserDbResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long? UserId { get; set; }
}
