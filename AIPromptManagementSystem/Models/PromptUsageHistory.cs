using Azure.Data.Tables;
using Azure;

namespace AIPromptManagementSystem.Models
{
    public class PromptUsageHistory : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty; 
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string PromptId { get; set; } = string.Empty;
        public string PromptTitle { get; set;} = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PromptText { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public PromptRatingEnum Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string AITool { get; set; } = string.Empty;
        public PromptCategoryEnum Category { get; set; } = PromptCategoryEnum.CodeGeneration;
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    }
}
