using AIPromptManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace AIPromptManagementSystem.DTOs
{
    public class CreatePromptDto
    {
        public string PromptId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prompt text is required")]
        [MinLength(20, ErrorMessage = "Prompt text must be at least 20 characters long")]
        public string PromptText { get; set; }
        public PromptCategoryEnum Category { get; set; } = PromptCategoryEnum.CodeGeneration;
        public string AITool { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
    }
}
