namespace TICTACTOEAPI.Models.DTO
{
    public class CreateGameRequest
    {
        public string Mode { get; set; } = "TwoPlayer"; // "TwoPlayer" | "VsComputer"
    }
}
