using System.ComponentModel.DataAnnotations;

namespace AIPromptManagementSystem.DTOs
{
    public class CreatePromptVersionDto
    {
        public string PromptId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prompt text is required")]
        [MinLength(20, ErrorMessage = "Prompt text must be at least 20 characters long")]
        public string PromptText { get; set; }
        public string ChangeNote { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
