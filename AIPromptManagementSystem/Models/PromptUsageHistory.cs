using Azure;
using Azure.Data.Tables;
using System.Runtime.Serialization;

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
        public string ChangeNote { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public int Version { get; set; }
        public PromptRatingEnum Rating { get; set; }
        public double AverageRating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string AITool { get; set; } = string.Empty;
        //public PromptCategoryEnum Category { get; set; } = PromptCategoryEnum.CodeGeneration;
        //public string Category
        //{
        //    get => CategoryValue.ToString();
        //    set => CategoryValue = Enum.Parse<PromptCategoryEnum>(value);
        //}

        // Це поле реально зберігається в Azure Table Storage
        public string Category { get; set; } = PromptCategoryEnum.CodeGeneration.ToString();

        // Це зручна обгортка для роботи з enum у коді
        [IgnoreDataMember] // щоб не дублювати в таблиці
        public PromptCategoryEnum CategoryValue
        {
            get => Enum.Parse<PromptCategoryEnum>(Category);
            set => Category = value.ToString();
        }
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    }
}
