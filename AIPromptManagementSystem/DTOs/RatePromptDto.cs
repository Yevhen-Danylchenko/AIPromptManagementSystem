using AIPromptManagementSystem.Models;

namespace AIPromptManagementSystem.DTOs
{
    public class RatePromptDto
    {
        public string PromptId { get; set; }
        public string UserName { get; set; }
        public PromptRatingEnum Rating { get; set; } = PromptRatingEnum.Погано;
        public string Comment { get; set; } = string.Empty;
    }
}
