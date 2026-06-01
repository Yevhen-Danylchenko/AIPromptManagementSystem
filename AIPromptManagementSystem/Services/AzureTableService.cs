using AIPromptManagementSystem.DTOs;
using AIPromptManagementSystem.Models;
using Azure;
using Azure.Data.Tables;

namespace AIPromptManagementSystem.Services
{
    public class AzureTableService
    {
        readonly TableClient _tableClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableService"/> class.
        /// </summary>
        /// <param name="configuration"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public AzureTableService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            var tableName = configuration["AzureStorage:TableName"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Azure Table Storage connection string is not configured.");
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new InvalidOperationException("Azure Table Storage table name is not configured.");
            }
            _tableClient = new TableClient(connectionString, tableName);
            _tableClient.CreateIfNotExists();
        }

        /// <summary>
        /// Retrieves all PromptUsageHistory entities from the underlying table storage asynchronously and returns them
        /// as a list.
        /// </summary>
        /// <remarks>Enumerates the table via QueryAsync and materializes all results into memory; this
        /// may incur high memory usage for large result sets.</remarks>
        /// <returns>A Task whose result is a List of PromptUsageHistory containing all retrieved entities.</returns>
        public async Task<List<PromptUsageHistory>> GetAllPromptsAsync()
        {
            var prompts = new List<PromptUsageHistory>();
            await foreach (var prompt in _tableClient.QueryAsync<PromptUsageHistory>())
            {
                prompts.Add(prompt);
            }
            return prompts.ToList();
        }

        /// <summary>
        /// Asynchronously retrieves PromptUsageHistory entities that 
        /// match the specified promptId from the underlying
        /// </summary>
        /// <param name="promptId"></param>
        /// <returns></returns>
        public async Task<List<PromptUsageHistory>> GetPromptByIdAsync(string promptId)
        {
            var filter = TableClient.CreateQueryFilter<PromptUsageHistory>(p => p.PromptId == promptId);
            var prompts = new List<PromptUsageHistory>();
            await foreach (var prompt in _tableClient.QueryAsync<PromptUsageHistory>(filter))
            {
                prompts.Add(prompt);
            }
            return prompts.ToList();
        }

        /// <summary>
        /// Adds a prompt usage record to table storage asynchronously using the values from the provided
        /// CreatePromptDto.
        /// </summary>
        /// <remarks>Generates a new RowKey, sets PartitionKey to 'PromptUsageHistory', sets UsedAt to the
        /// current UTC time, and persists the entity via the table client. Exceptions from the table client propagate
        /// to the caller.</remarks>
        /// <param name="obj">The DTO containing prompt details (PromptId, Title, Description, PromptText, Category, AITool, Author) used
        /// to construct the usage record.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        public async Task AddPromptAsync(CreatePromptDto obj)
        {
            var prompt = new PromptUsageHistory
            {
                PartitionKey = "PromptUsageHistory",
                RowKey = Guid.NewGuid().ToString(),
                PromptId = obj.PromptId,
                PromptTitle = obj.Title,
                Description = obj.Description,
                PromptText = obj.PromptText,
                Category = obj.Category.ToString(),
                AITool = obj.AITool,
                UserName = obj.Author,
                UsedAt = DateTime.UtcNow
            };
            await _tableClient.AddEntityAsync(prompt);
        }

        /// <summary>
        /// Asynchronously adds a new version of a prompt to table 
        /// storage based on the provided CreatePromptVersionDto. The method 
        /// first checks for the existence of a base prompt with the specified PromptId, 
        /// then retrieves all existing versions to determine the next version number 
        /// and calculate the average rating. Finally, it creates and persists a 
        /// new PromptUsageHistory entity representing the new version. Exceptions are 
        /// thrown if the base prompt does not exist or if there are issues with table 
        /// storage operations. 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<PromptUsageHistory> AddNewVersionPromptAsync(CreatePromptVersionDto obj)
        {
            // Перевіряємо, чи існує базовий промпт
            var filterBase = TableClient.CreateQueryFilter<PromptUsageHistory>(
                p => p.PartitionKey == "PromptBase" && p.PromptId == obj.PromptId
            );

            var existingEntity = await _tableClient.QueryAsync<PromptUsageHistory>(filterBase).FirstOrDefaultAsync();
            if (existingEntity == null)
            {
                throw new InvalidOperationException($"No base entity found with PromptId: {obj.PromptId}");
            }

            // Витягуємо всі версії для цього PromptId
            var filter = TableClient.CreateQueryFilter<PromptUsageHistory>(
                p => p.PartitionKey == "PromptVersionHistory" && p.PromptId == obj.PromptId
            );

            int maxVersion = 0;
            var ratings = new List<int>();

            await foreach (var entity in _tableClient.QueryAsync<PromptUsageHistory>(filter))
            {
                if (entity.Version > maxVersion)
                    maxVersion = entity.Version;

                if ((int)entity.Rating > 0)
                    ratings.Add((int)entity.Rating);
            }

            double averageRating = ratings.Count > 0 ? ratings.Average() : 0.0;

            // Створюємо нову версію
            var newPromptVersion = new PromptUsageHistory
            {
                PartitionKey = "PromptVersionHistory",
                RowKey = Guid.NewGuid().ToString(),
                PromptId = obj.PromptId,
                PromptTitle = existingEntity.PromptTitle, 
                Description = obj.ChangeNote,
                UserName = obj.CreatedBy,
                UsedAt = DateTime.UtcNow,
                Version = maxVersion + 1,
                AverageRating = averageRating
            };

            await _tableClient.AddEntityAsync(newPromptVersion);
            return newPromptVersion;
        }


        /// <summary>
        /// Adds a new rating for a specific prompt to table 
        /// storage asynchronously based on the provided PromptRating and  
        /// </summary>
        /// <param name="promptId"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<PromptUsageHistory> AddRatingAsync(string promptId, PromptRating obj)
        {
            // Перевіряємо, чи існує базовий промпт
            var ratingEntity = await _tableClient.GetEntityAsync<PromptUsageHistory>("PromptBase", promptId);

            if (ratingEntity == null || ratingEntity.Value == null)
            {
                throw new InvalidOperationException($"No base entity found with PromptId: {promptId}");
            }

            // Створюємо новий запис рейтингу
            var promptRating = new PromptUsageHistory
            {
                PartitionKey = "PromptRatings",
                RowKey = Guid.NewGuid().ToString(), // унікальний ключ для кожного рейтингу
                PromptId = promptId,
                UserName = obj.UserName,
                Rating = obj.Rating, 
                Comment = obj.Comment,
                UsedAt = DateTime.UtcNow
            };

            // Додаємо новий запис у таблицю
            await _tableClient.AddEntityAsync(promptRating);
            return promptRating;
        }


        /// <summary>
        /// Retrieves the usage history of a specific prompt by its 
        /// unique identifier (promptId) from table storage asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task<List<PromptUsageHistory>> GetPromptHistoryAsync()
        {
            var filter = TableClient.CreateQueryFilter<PromptUsageHistory>(
                p => p.PartitionKey == "PromptVersionHistory");

            var result = new List<PromptUsageHistory>();

            await foreach (var entity in _tableClient.QueryAsync<PromptUsageHistory>(filter))
            {
                result.Add(entity);
            }

            return result.OrderByDescending(h => h.UsedAt).ToList();
        }

        /// <summary>
        /// Asynchronously retrieves detailed information about a 
        /// specific prompt usage history entry by its unique identifier (id) 
        /// from table storage. The method attempts to fetch the entity with 
        /// the given id, and if found, maps its properties to a PromptDetailsDto 
        /// object which is then returned. If no entity is found with the specified id, 
        /// the method returns null. Exceptions from the table client are handled 
        /// to return null in case of a 404 Not Found status, while other exceptions 
        /// will propagate to the caller.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PromptDetailsDto?> PromptDetailsAsync(string id)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<PromptUsageHistory>("PromptUsageHistory", id);
                var entity = response.Value;

                // Мапимо сутність у DTO
                var dto = new PromptDetailsDto
                {
                    Id = entity.RowKey,
                    Title = entity.PromptTitle,
                    Description = entity.Description,
                    CurrentText = entity.PromptText,
                    Category = entity.Category,
                    AITool = entity.AITool,
                    Author = entity.UserName,
                    Rating = entity.Rating,
                    AverageRating = entity.AverageRating,
                    CurrentVersion = entity.Version,
                    CreatedAt = entity.UsedAt,
                    UpdatedAt = entity.Timestamp?.UtcDateTime ?? DateTime.UtcNow
                };

                return dto;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        /// <summary>
        /// Searches for PromptUsageHistory entries where the 
        /// PromptTitle or Description contains the specified query string.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<List<PromptUsageHistory>> SearchPromptAsync(string query)
        {
            // Фільтруємо лише по PartitionKey на сервері
            var filter = TableClient.CreateQueryFilter<PromptUsageHistory>(
                p => p.PartitionKey == "PromptVersionHistory"
            );

            var results = new List<PromptUsageHistory>();

            await foreach (var entity in _tableClient.QueryAsync<PromptUsageHistory>(filter))
            {
                // Перевірка на клієнті: пошук по частині слова в Title або Description
                if ((!string.IsNullOrEmpty(entity.PromptTitle) &&
                     entity.PromptTitle.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(entity.Description) &&
                     entity.Description.Contains(query, StringComparison.OrdinalIgnoreCase)))
                {
                    results.Add(entity);
                }
            }

            return results;
        }

        /// <summary>
        /// Retrieves PromptUsageHistory entities that match the specified category from table storage.
        /// </summary>
        /// <remarks>Performs an asynchronous query using TableClient.CreateQueryFilter and enumerates the
        /// results.</remarks>
        /// <param name="category">The category used to filter PromptUsageHistory entities.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of PromptUsageHistory
        /// entities that match the specified category.</returns>
        public async Task<List<PromptUsageHistory>> FilterByCategoryAsync(PromptCategoryEnum category)
        {
            var filter = TableClient.CreateQueryFilter<PromptUsageHistory>(p => p.Category == category.ToString());

            var results = new List<PromptUsageHistory>();

            await foreach (var entity in _tableClient.QueryAsync<PromptUsageHistory>(filter))
            {
                results.Add(entity);
            }

            return results;
        }

        /// <summary>
        /// Retrieves all PromptUsageHistory entities matching the specified AI tool.
        /// </summary>
        /// <remarks>Performs an asynchronous table query via TableClient and materializes the results
        /// into a List.</remarks>
        /// <param name="aiTool">AI tool identifier used to filter the PromptUsageHistory entities.</param>
        /// <returns>A Task whose result is a List<PromptUsageHistory> containing the matching entities.</returns>
        public async Task<List<PromptUsageHistory>> FilterByAIToolAsync(string aiTool)
        {
            var filter = TableClient.CreateQueryFilter<PromptUsageHistory>(p => p.AITool == aiTool);
            var results = new List<PromptUsageHistory>();
            await foreach (var entity in _tableClient.QueryAsync<PromptUsageHistory>(filter))
            {
                results.Add(entity);
            }

            return results;
        }

        /// <summary>
        /// Retrieves PromptUsageHistory entries with a Rating greater than zero and returns them ordered by Rating in
        /// descending order.
        /// </summary>
        /// <remarks>Filters entities server-side for Rating > 0 and performs an in-memory descending sort
        /// by Rating after enumeration.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of PromptUsageHistory
        /// ordered by Rating in descending order.</returns>
        public async Task<List<PromptUsageHistory>> SortedByRatingAsync()
        {
            var filter = TableClient.CreateQueryFilter<PromptUsageHistory>(p => p.Rating > 0);
            var results = new List<PromptUsageHistory>();
            await foreach (var entity in _tableClient.QueryAsync<PromptUsageHistory>(filter))
            {
                results.Add(entity);
            }
            return results.OrderByDescending(e => e.Rating).ToList();
        }

        /// <summary>
        /// Searches for PromptUsageHistory entries where the 
        /// PromptTitle contains the specified title fragment.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public async Task<List<PromptUsageHistory>> SearchForTitle(string title)
        {
            var filter = TableClient.CreateQueryFilter<PromptUsageHistory>(p => p.PromptTitle.Contains(title));
            var results = new List<PromptUsageHistory>();
            await foreach (var entity in _tableClient.QueryAsync<PromptUsageHistory>(filter))
            {
                results.Add(entity);
            }
            return results;
        }
    }
}
