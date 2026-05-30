using AIPromptManagementSystem.Models;

namespace AIPromptManagementSystem.DTOs
{
    public class CreatePromptDto
    {
        public string PromptId { get; set; } = string.Empty;
        public string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public string PromptText { get; set; } = string.Empty;
        public PromptCategoryEnum Category { get; set; } = PromptCategoryEnum.CodeGeneration;
        public string AITool { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
    }
}
