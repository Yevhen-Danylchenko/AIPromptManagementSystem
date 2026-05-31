using System;
using System.ComponentModel.DataAnnotations;

namespace AIPromptManagementSystem.Models
{
    public class Prompt
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "CurrentText is required")]
        [MinLength(20, ErrorMessage = "CurrentText must be at least 20 characters long")]
        public string CurrentText { get; set; }
        public PromptCategoryEnum Category { get; set; } = PromptCategoryEnum.CodeGeneration;
        public string AITool { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int CurrentVersion { get; set; } = 0;
        public double AverageRating { get; set; } = 0.0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsArchived { get; set; } = false;
    }
}
