namespace AIPromptManagementSystem.Models
{
    public class PromptVersion
    {
        public int Id { get; set; }
        public string PromptId { get; set; } = string.Empty;
        public int VersionNumber { get; set; } = 0;
        public string PromptText { get; set; } = string.Empty;
        public string ChangeNote { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
