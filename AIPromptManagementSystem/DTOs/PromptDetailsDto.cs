using AIPromptManagementSystem.Models;

namespace AIPromptManagementSystem.DTOs
{
    public class PromptDetailsDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public string CurrentText { get; set; } = string.Empty;
        public PromptCategoryEnum Category { get; set; } = PromptCategoryEnum.CodeGeneration;
        public string AITool { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public PromptRatingEnum Rating { get; set; } = PromptRatingEnum.Погано;
        public double AverageRating { get; set; } = 0.0;
        public int CurrentVersion { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
