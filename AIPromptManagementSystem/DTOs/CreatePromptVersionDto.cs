namespace AIPromptManagementSystem.DTOs
{
    public class CreatePromptVersionDto
    {
        public string PromptId { get; set; } = string.Empty;
        public string PromptText { get; set; } = string.Empty;
        public string ChangeNote { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
