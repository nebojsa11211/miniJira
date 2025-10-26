using Microsoft.EntityFrameworkCore;
using MiniJira.Data;

namespace MiniJira.Services
{
    public interface IDataResetService
    {
        Task ResetAllDataAsync();
        Task SeedTestTasksAsync(int totalCount = 100, double donePercentage = 0.7);
    }

    public class DataResetService : IDataResetService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataResetService> _logger;

        public DataResetService(ApplicationDbContext context, ILogger<DataResetService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ResetAllDataAsync()
        {
            _logger.LogWarning("Starting data reset operation...");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Delete work order rows
                var workOrderRows = await _context.WorkOrderRows.ToListAsync();
                _context.WorkOrderRows.RemoveRange(workOrderRows);
                _logger.LogInformation("Deleted {Count} work order rows", workOrderRows.Count);

                // 2. Delete work order headers
                var workOrderHeaders = await _context.WorkOrderHeaders.ToListAsync();
                _context.WorkOrderHeaders.RemoveRange(workOrderHeaders);
                _logger.LogInformation("Deleted {Count} work order headers", workOrderHeaders.Count);

                // 3. Delete task comments
                var taskComments = await _context.TaskComments.ToListAsync();
                _context.TaskComments.RemoveRange(taskComments);
                _logger.LogInformation("Deleted {Count} task comments", taskComments.Count);

                // 4. Delete task attachments
                var taskAttachments = await _context.TaskAttachments.ToListAsync();
                _context.TaskAttachments.RemoveRange(taskAttachments);
                _logger.LogInformation("Deleted {Count} task attachments", taskAttachments.Count);

                // 5. Delete task owner histories
                var taskOwnerHistories = await _context.TaskOwnerHistories.ToListAsync();
                _context.TaskOwnerHistories.RemoveRange(taskOwnerHistories);
                _logger.LogInformation("Deleted {Count} task owner histories", taskOwnerHistories.Count);

                // 6. Delete task column histories
                var taskColumnHistories = await _context.TaskColumnHistories.ToListAsync();
                _context.TaskColumnHistories.RemoveRange(taskColumnHistories);
                _logger.LogInformation("Deleted {Count} task column histories", taskColumnHistories.Count);

                // 7. Delete all tasks
                var tasks = await _context.Tasks.ToListAsync();
                _context.Tasks.RemoveRange(tasks);
                _logger.LogInformation("Deleted {Count} tasks", tasks.Count);

                // 8. Delete custom column translations (keep system column translations)
                var customColumnIds = await _context.Columns
                    .Where(c => !c.IsSystem)
                    .Select(c => c.Id)
                    .ToListAsync();

                if (customColumnIds.Any())
                {
                    var customColumnTranslations = await _context.ColumnTranslations
                        .Where(ct => customColumnIds.Contains(ct.ColumnId))
                        .ToListAsync();
                    _context.ColumnTranslations.RemoveRange(customColumnTranslations);
                    _logger.LogInformation("Deleted {Count} custom column translations", customColumnTranslations.Count);
                }

                // 9. Delete custom columns (preserve system columns)
                var customColumns = await _context.Columns
                    .Where(c => !c.IsSystem)
                    .ToListAsync();
                _context.Columns.RemoveRange(customColumns);
                _logger.LogInformation("Deleted {Count} custom columns", customColumns.Count);

                // Save all changes
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogWarning("Data reset completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during data reset operation");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task SeedTestTasksAsync(int totalCount = 100, double donePercentage = 0.7)
        {
            _logger.LogInformation("Starting to seed {Count} test tasks with {Percentage}% done", totalCount, donePercentage * 100);

            var random = new Random();
            var now = DateTime.UtcNow;

            // Use hardcoded default column GUIDs (defined in migrations)
            // Note: "In Progress" is no longer a system column but still exists as a default column
            var todoColumnId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var inProgressColumnId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var doneColumnId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            // Verify these columns exist in the database
            var columnIds = new[] { todoColumnId, inProgressColumnId, doneColumnId };
            var existingColumns = await _context.Columns
                .Where(c => columnIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            if (existingColumns.Count != 3)
            {
                var missing = columnIds.Except(existingColumns).ToList();
                throw new InvalidOperationException(
                    $"Required default columns not found in database. Missing: {string.Join(", ", missing)}. " +
                    "Database may not be properly initialized.");
            }

            _logger.LogInformation("Using columns: ToDo={TodoId}, InProgress={InProgressId}, Done={DoneId}",
                todoColumnId, inProgressColumnId, doneColumnId);

            var tasks = new List<Models.Task>();
            var histories = new List<Models.TaskColumnHistory>();

            // Calculate how many tasks should be in each status
            int doneCount = (int)(totalCount * donePercentage);
            int remainingCount = totalCount - doneCount;
            int inProgressCount = remainingCount / 3;
            int todoCount = remainingCount - inProgressCount;

            _logger.LogInformation("Creating {Todo} ToDo, {InProgress} InProgress, {Done} Done tasks",
                todoCount, inProgressCount, doneCount);

            var customers = new[] { "Acme Corp", "TechStart Inc", "Global Solutions", "Innovation Labs", "Future Systems", null };
            var titlePrefixes = new[] { "Implement", "Fix", "Update", "Refactor", "Add", "Remove", "Optimize", "Design", "Test", "Deploy" };
            var titleSubjects = new[]
            {
                "user authentication", "dashboard layout", "API endpoint", "database schema", "payment integration",
                "search functionality", "email notifications", "file upload", "data validation", "caching layer",
                "logging system", "error handling", "responsive design", "performance monitoring", "security patch"
            };

            for (int i = 1; i <= totalCount; i++)
            {
                Models.TaskStatus status;
                Guid columnId;

                // Assign status based on distribution
                if (i <= doneCount)
                {
                    status = Models.TaskStatus.Done;
                    columnId = doneColumnId;
                }
                else if (i <= doneCount + inProgressCount)
                {
                    status = Models.TaskStatus.InProgress;
                    columnId = inProgressColumnId;
                }
                else
                {
                    status = Models.TaskStatus.ToDo;
                    columnId = todoColumnId;
                }

                var priority = (Models.TaskPriority)random.Next(0, 3);
                var customer = customers[random.Next(customers.Length)];
                var titlePrefix = titlePrefixes[random.Next(titlePrefixes.Length)];
                var titleSubject = titleSubjects[random.Next(titleSubjects.Length)];

                // Create tasks with varied creation dates (last 30 days)
                var daysAgo = random.Next(0, 30);
                var createdAt = now.AddDays(-daysAgo).AddHours(-random.Next(0, 24));
                var updatedAt = status == Models.TaskStatus.Done
                    ? createdAt.AddHours(random.Next(1, 48))
                    : createdAt.AddHours(random.Next(1, 12));

                var task = new Models.Task
                {
                    Id = Guid.NewGuid(),
                    Title = $"{titlePrefix} {titleSubject} #{i}",
                    Description = GenerateDescription(titlePrefix, titleSubject, i),
                    Status = status,
                    Priority = priority,
                    Customer = customer,
                    ColumnId = columnId,
                    CreatedAt = createdAt,
                    UpdatedAt = updatedAt,
                    IsDeleted = false,
                    IsHidden = false
                };

                tasks.Add(task);

                // Create column history entry
                var history = new Models.TaskColumnHistory
                {
                    Id = Guid.NewGuid(),
                    TaskId = task.Id,
                    FromColumnId = null, // Initial assignment, no previous column
                    ToColumnId = columnId,
                    ChangedAt = createdAt,
                    ChangedBy = null // Mock system doesn't track user for seed data
                };

                histories.Add(history);
            }

            // Add all tasks and histories to context
            await _context.Tasks.AddRangeAsync(tasks);
            await _context.TaskColumnHistories.AddRangeAsync(histories);

            // Save changes
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {Count} test tasks", totalCount);
        }

        private string GenerateDescription(string prefix, string subject, int taskNumber)
        {
            var descriptions = new Dictionary<string, string[]>
            {
                ["Implement"] = new[]
                {
                    "This task involves implementing {0} from scratch. We need to ensure proper error handling and validation.",
                    "We need to build {0} according to the specifications. Include unit tests and documentation.",
                    "Create a robust implementation of {0}. Follow best practices and coding standards."
                },
                ["Fix"] = new[]
                {
                    "There's a critical bug in {0} that needs to be addressed. Users are experiencing issues.",
                    "Resolve the issue with {0}. Root cause analysis required before implementing the fix.",
                    "Bug reported in {0}. Investigate and apply appropriate fix. Add regression test."
                },
                ["Update"] = new[]
                {
                    "The {0} needs to be updated to support new requirements. Ensure backward compatibility.",
                    "Modernize {0} to use latest patterns and practices. Update documentation accordingly.",
                    "Enhance {0} with additional features requested by stakeholders."
                },
                ["Refactor"] = new[]
                {
                    "The {0} code needs refactoring for better maintainability. No functional changes expected.",
                    "Improve the architecture of {0}. Reduce complexity and improve readability.",
                    "Clean up {0} implementation. Remove technical debt and apply SOLID principles."
                },
                ["Add"] = new[]
                {
                    "Add new feature for {0}. This is a high-priority request from the product team.",
                    "Introduce {0} to the system. Ensure it integrates well with existing components.",
                    "Extend the application with {0}. Include proper error handling and logging."
                }
            };

            var templates = descriptions.ContainsKey(prefix)
                ? descriptions[prefix]
                : new[] { "Work on {0}. Task #{1} in the backlog." };

            var template = templates[new Random(taskNumber).Next(templates.Length)];
            return string.Format(template, subject, taskNumber);
        }
    }
}
