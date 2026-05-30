using AIPromptManagementSystem.DTOs;
using AIPromptManagementSystem.Models;
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
            var connectionString = configuration.GetConnectionString("AzureTableStorage");
            var tableName = configuration["AzureTableStorage:TableName"];

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
        public async Task<List<PromptUsageHistory>> GetAllPromptAsync()
        {
            var prompts = new List<PromptUsageHistory>();
            await foreach (var prompt in _tableClient.QueryAsync<PromptUsageHistory>())
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
                Category = obj.Category,
                AITool = obj.AITool,
                UserName = obj.Author,
                UsedAt = DateTime.UtcNow
            };
            await _tableClient.AddEntityAsync(prompt);
        }

        /// <summary>
        /// Adds a new prompt version record to table storage for an existing prompt asynchronously.
        /// </summary>
        /// <remarks>Generates a new PromptUsageHistory entity with a unique RowKey, sets PartitionKey to
        /// 'PromptVersionHistory', populates metadata (PromptTitle, Description, UserName, UsedAt), and persists the
        /// entity to the table.</remarks>
        /// <param name="obj">CreatePromptVersionDto containing PromptId, CreatedBy, and ChangeNote used to create the new version.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no base prompt entity exists with the specified PromptId.</exception>
        public async Task AddNewVersionPromptAsync(CreatePromptVersionDto obj)
        {
            // Перевіряємо, чи існує базовий промпт (щоб не створювати версію для неіснуючого)
            var existingEntity = await _tableClient.GetEntityAsync<PromptUsageHistory>("PromptBase", obj.PromptId);

            if (existingEntity == null || existingEntity.Value == null)
            {
                throw new InvalidOperationException($"No base entity found with PromptId: {obj.PromptId}");
            }

            // Створюємо нову версію з новим RowKey
            var newPromptVersion = new PromptUsageHistory
            {
                PartitionKey = "PromptVersionHistory",
                RowKey = Guid.NewGuid().ToString(), // новий унікальний ключ
                PromptId = obj.PromptId,
                PromptTitle = $"Version of {obj.PromptId}, created by {obj.CreatedBy}",
                Description = obj.ChangeNote,
                UserName = obj.CreatedBy,
                UsedAt = DateTime.UtcNow
            };

            // Додаємо новий запис у таблицю
            await _tableClient.AddEntityAsync(newPromptVersion);
        }


        /// <summary>
        /// Adds a new rating for the specified prompt to table storage asynchronously.
        /// </summary>
        /// <remarks>Creates a PromptUsageHistory entry with PartitionKey 'PromptRatings', a new GUID
        /// RowKey, and UsedAt set to UTC now; requires an existing base entity under partition 'PromptBase'.</remarks>
        /// <param name="promptId">Identifier of the base prompt to which the rating applies.</param>
        /// <param name="obj">PromptRating containing the user name, rating value, and optional comment to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no base entity exists with the specified promptId.</exception>
        public async Task AddRatingAsync(string promptId, PromptRating obj)
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
        }


        /// <summary>
        /// Asynchronously retrieves usage history for the specified prompt from the configured table storage.
        /// </summary>
        /// <remarks>Performs an asynchronous query using the configured TableClient and returns an empty
        /// list if no entries are found.</remarks>
        /// <param name="promptId">The identifier of the prompt to retrieve usage history for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of PromptUsageHistory
        /// entries for the specified prompt.</returns>
        public async Task<List<PromptUsageHistory>> GetPromptUsageHistoryAsync(string promptId)
        {
            var filter = TableClient.CreateQueryFilter<PromptUsageHistory>(p => p.PromptId == promptId);
            var usageHistory = new List<PromptUsageHistory>();
            await foreach (var entity in _tableClient.QueryAsync<PromptUsageHistory>(filter))
            {
                usageHistory.Add(entity);
            }
            return usageHistory;
        }

        /// <summary>
        /// Retrieves details for the specified prompt, including the most recent usage record and the average of
        /// recorded ratings.
        /// </summary>
        /// <remarks>AverageRating is 0.0 when there are no positive ratings. CreatedAt is taken from the
        /// latest usage record's UsedAt; UpdatedAt is taken from the entity Timestamp or defaults to UTC now. The
        /// method queries the configured table client and iterates all matching usage records to determine the latest
        /// entry and aggregate ratings.</remarks>
        /// <param name="promptId">Unique identifier of the prompt to retrieve.</param>
        /// <returns>A PromptDetailsDto with Id, Title, Description, Author, AverageRating, CreatedAt, and UpdatedAt populated,
        /// or null if no usage history exists for the specified prompt.</returns>
        public async Task<PromptDetailsDto?> PromptDetailsAsync(string promptId)
        {
            var filter = TableClient.CreateQueryFilter<PromptUsageHistory>(p => p.PromptId == promptId);

            var ratings = new List<int>();
            PromptUsageHistory? latestEntity = null;

            await foreach (var entity in _tableClient.QueryAsync<PromptUsageHistory>(filter))
            {
                // зберігаємо останній запис (наприклад, за часом використання)
                if (latestEntity == null || entity.UsedAt > latestEntity.UsedAt)
                {
                    latestEntity = entity;
                }

                // збираємо всі рейтинги
                if (entity.Rating > 0)
                {
                    ratings.Add((int)entity.Rating);
                }
            }

            if (latestEntity == null)
                return null;

            return new PromptDetailsDto
            {
                Id = latestEntity.RowKey,
                Title = latestEntity.PromptTitle,
                Description = latestEntity.Description,
                Author = latestEntity.UserName,
                AverageRating = ratings.Count > 0 ? ratings.Average() : 0.0,
                CreatedAt = latestEntity.UsedAt,
                UpdatedAt = latestEntity.Timestamp?.DateTime ?? DateTime.UtcNow
            };
        }

        /// <summary>
        /// Searches prompt usage history entries whose PromptTitle or Description contains the specified query.
        /// </summary>
        /// <remarks>Performs a server-side table query via TableClient and materializes all matching
        /// entities into a List.</remarks>
        /// <param name="query">The search text to match against PromptTitle and Description.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of PromptUsageHistory
        /// entities that match the query.</returns>
        public async Task<List<PromptUsageHistory>> SearchPromptAsync(string query)
        {
            var filter = TableClient.CreateQueryFilter<PromptUsageHistory>(p => p.PromptTitle.Contains(query) || p.Description.Contains(query));
            var searchResults = new List<PromptUsageHistory>();

            await foreach (var entity in _tableClient.QueryAsync<PromptUsageHistory>(filter))
            {
                searchResults.Add(entity);
            }

            return searchResults;
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
            var filter = TableClient.CreateQueryFilter<PromptUsageHistory>(p => p.Category == category);

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
    }
}
