using Microsoft.AspNetCore.Mvc;
using AIPromptManagementSystem.Services;

namespace AIPromptManagementSystem.Controllers
{    
    public class HistoryController : Controller
    {
        private readonly AzureTableService _azureTableService;

        public HistoryController(AzureTableService azureTableService)
        {
            _azureTableService = azureTableService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var history = await _azureTableService.GetPromptHistoryAsync();

            return View(history);
        }
    }
}
