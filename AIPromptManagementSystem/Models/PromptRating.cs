namespace AIPromptManagementSystem.Models
{
    public class PromptRating
    {
        public int Id { get; set; }
        public string PromptId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public PromptRatingEnum Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
