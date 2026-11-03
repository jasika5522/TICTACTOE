using Microsoft.AspNetCore.Mvc;

namespace PERSONALAPP.Controllers
{
    public class MyPortfolioController : Controller
    {
        public async Task<IActionResult> MyPortfolio()
        {
            return View();
        }
    }
}
