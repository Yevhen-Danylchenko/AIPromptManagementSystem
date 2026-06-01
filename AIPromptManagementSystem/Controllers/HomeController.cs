using AIPromptManagementSystem.Models;
using AIPromptManagementSystem.Services;
using AIPromptManagementSystem.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AIPromptManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        readonly AzureTableService _azureTableService;

        public HomeController(AzureTableService azureTableService)
        {
            _azureTableService = azureTableService;
        }

        /// <summary>
        /// Displays the list of AI prompts on the home page.
        /// </summary>
        /// <returns>View result containing the list of prompts.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var prompts = await _azureTableService.GetAllPromptsAsync();
            var promptDTOs = prompts.Select(p => new PromptListDto
            {
                Id = p.RowKey,
                Title = p.PromptTitle,
                Category = Enum.Parse<PromptCategoryEnum>(p.Category),
                AITool = p.AITool,
                Author = p.UserName,
                AverageRating = p.AverageRating,
                CurrentVersion = p.Version 
            }).ToList();
            return View(promptDTOs);
        }

        /// <summary>
        /// Displays the details of a specific AI prompt based on the provided ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var prompt = await _azureTableService.PromptDetailsAsync(id);
            if (prompt == null)
            {
                return NotFound();
            }
            return View(prompt);
        }

        /// <summary>
        /// Displays the form for creating a new AI prompt.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        /// <summary>
        /// Handles the submission of the form for creating a new 
        /// AI prompt. Validates the input and adds the prompt to the 
        /// Azure Table Storage if valid. Redirects to the home page 
        /// after successful creation.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create(CreatePromptDto model)
        {
            if (ModelState.IsValid)
            {
                await _azureTableService.AddPromptAsync(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        /// <summary>
        /// Displays the form for creating a new version of an existing AI prompt.
        /// </summary>
        /// <param name="promptId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> CreateVersion(string promptId)
        {
            var model = new CreatePromptVersionDto
            {
                PromptId = promptId
            };
            return View(model);
        }

        /// <summary>
        /// Handles the submission of the form for creating a 
        /// new version of an existing AI prompt. Validates the 
        /// input and adds the new version to the Azure Table Storage if valid. 
        /// Redirects to the home page after successful creation.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateVersion(CreatePromptVersionDto model)
        {
            if (ModelState.IsValid)
            {
                await _azureTableService.AddNewVersionPromptAsync(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        /// <summary>
        /// Displays the form for rating an AI prompt based on the provided ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Rate(string id)
        {
            var prompt = await _azureTableService.GetPromptByIdAsync(id);
            return View(prompt);
        }

        /// <summary>
        /// Handles the submission of the form for rating an AI prompt. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rating"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Rate(string id, int rating)
        {
            if (rating < 1 || rating > 5)
            {
                ModelState.AddModelError("Rating", "Rating must be between 1 and 5.");
                var prompt = await _azureTableService.GetPromptByIdAsync(id);
                return View(prompt);
            }
            await _azureTableService.AddRatingAsync(id, new PromptRating { Rating = (PromptRatingEnum)rating });
            return RedirectToAction("Details", new { id });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
