using AIPromptManagementSystem.Models;

namespace AIPromptManagementSystem.DTOs
{
    public class PromptListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public PromptCategoryEnum Category { get; set; } = PromptCategoryEnum.CodeGeneration;
        public string AITool { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public double AverageRating { get; set; } = 0.0;
        public string CurrentVersion { get; set; } = string.Empty;
    }
}
