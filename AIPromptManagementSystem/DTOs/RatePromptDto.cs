using AIPromptManagementSystem.Models;

namespace AIPromptManagementSystem.DTOs
{
    public class RatePromptDto
    {
        public string PromptId { get; set; }
        public string UserName { get; set; }
        public PromptRatingEnum Rating { get; set; } 
        public string Comment { get; set; } = string.Empty;
    }
}
