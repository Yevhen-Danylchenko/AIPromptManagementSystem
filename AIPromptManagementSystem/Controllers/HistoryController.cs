using AIPromptManagementSystem.Models;
using AIPromptManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIPromptManagementSystem.Controllers
{    
    public class HistoryController : Controller
    {
        private readonly AzureTableService _azureTableService;

        public HistoryController(AzureTableService azureTableService)
        {
            _azureTableService = azureTableService;
        }

        /// <summary>
        /// Displays the prompt usage history with optional 
        /// filtering by query, category, or AI tool.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="category"></param>
        /// <param name="aiTool"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index(string? query, PromptCategoryEnum? category, string? aiTool)
        {
            List<PromptUsageHistory> history;

            if (!string.IsNullOrEmpty(query))
            {
                history = await _azureTableService.SearchPromptAsync(query);
            }
            else if (category.HasValue)
            {
                history = await _azureTableService.FilterByCategoryAsync(category.Value);
            }
            else if (!string.IsNullOrEmpty(aiTool))
            {
                history = await _azureTableService.FilterByAIToolAsync(aiTool);
            }
            else
            {
                history = await _azureTableService.GetPromptHistoryAsync();
            }

            return View(history);
        }

        /// <summary>
        /// Displays the prompt usage history sorted by rating in descending order.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SortedByRating()
        {
            var history = await _azureTableService.SortedByRatingAsync();
            return View("Index", history); 
        }
    }
}
