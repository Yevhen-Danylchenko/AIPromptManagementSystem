using AIPromptManagementSystem.Models;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;

namespace AIPromptManagementSystem.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly TableClient _tableClient;

        public HealthController(IWebHostEnvironment env, TableClient tableClient)
        {
            _env = env;
            _tableClient = tableClient;
        }

        /// <summary>
        /// Gets application health information including application name, environment name, current UTC time, total
        /// prompts, and total usages.
        /// </summary>
        /// <remarks>Asynchronously queries the TableClient to count total prompts and prompt version
        /// usages (PartitionKey 'PromptVersionHistory').</remarks>
        /// <returns>A 200 OK result containing a JSON object with application name, environment, UtcTime, TotalPrompts, and
        /// TotalUsages.</returns>
        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            var totalPrompts = await _tableClient.QueryAsync<PromptUsageHistory>().CountAsync();
            var totalUsages = await _tableClient.QueryAsync<PromptUsageHistory>(p => p.PartitionKey == "PromptVersionHistory").CountAsync();

            return Ok(new
            {
                ApplicationName = "AI Prompt Management System",
                Environment = _env.EnvironmentName,
                UtcTime = DateTime.UtcNow,
                TotalPrompts = totalPrompts,
                TotalUsages = totalUsages
            });
        }
    }
}
