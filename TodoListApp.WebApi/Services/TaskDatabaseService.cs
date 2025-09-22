using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Contexts;
using TodoListApp.Services.Database.Entities;
using TodoListApp.WebApi.Models;
using Task = TodoListApp.WebApi.Models.Task;

namespace TodoListApp.WebApi.Services
{
    public class TaskDatabaseService : ITaskDatabaseService
    {
        private readonly TodoListDbContext _dbContext;
        private readonly ILogger<TaskDatabaseService> _logger;

        public TaskDatabaseService(TodoListDbContext dbContext, ILogger<TaskDatabaseService> logger)
        {
            this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Task>> GetTasksByTodoListIdAsync(int todoListId, int page = 1, int pageSize = 10)
        {
            try
            {
                var skip = (page - 1) * pageSize;
                var tasks = await this._dbContext.Tasks
                    .Where(t => t.TodoListId == todoListId)
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                return tasks.Select(entity => MapEntityToModel(entity));
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error retrieving tasks for todo list ID {todoListId}");
                throw;
            }
        }

        public async Task<int> GetTasksCountByTodoListIdAsync(int todoListId)
        {
            try
            {
                return await this._dbContext.Tasks
                    .Where(t => t.TodoListId == todoListId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error counting tasks for todo list ID {todoListId}");
                throw;
            }
        }

        public async Task<Task> GetTaskByIdAsync(int id)
        {
            try
            {
                var entity = await this._dbContext.Tasks.FindAsync(id);
                return entity != null ? MapEntityToModel(entity) : null;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error retrieving task with ID {id}");
                throw;
            }
        }

        public async Task<Task> AddTaskAsync(Task task)
        {
            ArgumentNullException.ThrowIfNull(task);

            try
            {
                // Check if the todo list exists
                var todoListExists = await this._dbContext.TodoLists.AnyAsync(tl => tl.Id == task.TodoListId);
                if (!todoListExists)
                {
                    throw new InvalidOperationException($"Todo list with ID {task.TodoListId} does not exist");
                }

                var entity = new TaskEntity
                {
                    Title = task.Title,
                    Description = task.Description,
                    TodoListId = task.TodoListId,
                    Status = task.Status,
                    DueDate = task.DueDate,
                    AssignedUserId = task.AssignedUserId,
                    CreatedAt = DateTime.UtcNow,
                    TagsJson = task.Tags?.Count > 0 ? JsonSerializer.Serialize(task.Tags) : null,
                    CommentsJson = task.Comments?.Count > 0 ? JsonSerializer.Serialize(task.Comments) : null
                };

                await this._dbContext.Tasks.AddAsync(entity);
                await this._dbContext.SaveChangesAsync();

                return MapEntityToModel(entity);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error adding task");
                throw;
            }
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            try
            {
                var entity = await this._dbContext.Tasks.FindAsync(id);
                if (entity == null)
                {
                    return false;
                }

                this._dbContext.Tasks.Remove(entity);
                await this._dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error deleting task with ID {id}");
                throw;
            }
        }

        public async Task<Task> UpdateTaskAsync(int id, Task task)
        {
            ArgumentNullException.ThrowIfNull(task);

            try
            {
                var entity = await this._dbContext.Tasks.FindAsync(id);
                if (entity == null)
                {
                    return null;
                }

                // Check if the todo list exists if it's changed
                if (entity.TodoListId != task.TodoListId)
                {
                    var todoListExists = await this._dbContext.TodoLists.AnyAsync(tl => tl.Id == task.TodoListId);
                    if (!todoListExists)
                    {
                        throw new InvalidOperationException($"Todo list with ID {task.TodoListId} does not exist");
                    }
                }

                entity.Title = task.Title;
                entity.Description = task.Description;
                entity.TodoListId = task.TodoListId;
                entity.Status = task.Status;
                entity.DueDate = task.DueDate;
                entity.AssignedUserId = task.AssignedUserId;

                // Update tags and comments if provided
                if (task.Tags?.Count > 0)
                {
                    entity.TagsJson = JsonSerializer.Serialize(task.Tags);
                }

                if (task.Comments?.Count > 0)
                {
                    entity.CommentsJson = JsonSerializer.Serialize(task.Comments);
                }

                this._dbContext.Tasks.Update(entity);
                await this._dbContext.SaveChangesAsync();

                return MapEntityToModel(entity);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error updating task with ID {id}");
                throw;
            }
        }

        public async Task<IEnumerable<Task>> GetOverdueTasksByTodoListIdAsync(int todoListId, int page = 1, int pageSize = 10)
        {
            try
            {
                var currentDate = DateTime.UtcNow;
                var skip = (page - 1) * pageSize;

                var tasks = await this._dbContext.Tasks
                    .Where(t => t.TodoListId == todoListId &&
                            t.DueDate.HasValue &&
                            t.DueDate < currentDate &&
                            t.Status != TodoListApp.WebApi.Models.TaskStatus.Completed)
                    .OrderBy(t => t.DueDate)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                return tasks.Select(entity => MapEntityToModel(entity));
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error retrieving overdue tasks for todo list ID {todoListId}");
                throw;
            }
        }

        public async Task<Task> AddTagToTaskAsync(int taskId, string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentException("Tag cannot be empty", nameof(tag));
            }

            try
            {
                var entity = await this._dbContext.Tasks.FindAsync(taskId);
                if (entity == null)
                {
                    return null;
                }

                // Get existing tags or create new list
                var tags = string.IsNullOrEmpty(entity.TagsJson)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(entity.TagsJson);

                // Add tag if not already exists
                if (!tags.Contains(tag))
                {
                    tags.Add(tag);
                    entity.TagsJson = JsonSerializer.Serialize(tags);

                    this._dbContext.Tasks.Update(entity);
                    await this._dbContext.SaveChangesAsync();
                }

                return MapEntityToModel(entity);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error adding tag to task with ID {taskId}");
                throw;
            }
        }

        public async Task<Task> RemoveTagFromTaskAsync(int taskId, string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentException("Tag cannot be empty", nameof(tag));
            }

            try
            {
                var entity = await this._dbContext.Tasks.FindAsync(taskId);
                if (entity == null)
                {
                    return null;
                }

                // Get existing tags
                if (!string.IsNullOrEmpty(entity.TagsJson))
                {
                    var tags = JsonSerializer.Deserialize<List<string>>(entity.TagsJson);

                    // Remove tag if exists
                    if (tags.Remove(tag))
                    {
                        entity.TagsJson = tags.Count > 0 ? JsonSerializer.Serialize(tags) : null;

                        this._dbContext.Tasks.Update(entity);
                        await this._dbContext.SaveChangesAsync();
                    }
                }

                return MapEntityToModel(entity);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error removing tag from task with ID {taskId}");
                throw;
            }
        }

        public async Task<Task> AddCommentToTaskAsync(int taskId, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                throw new ArgumentException("Comment cannot be empty", nameof(comment));
            }

            try
            {
                var entity = await this._dbContext.Tasks.FindAsync(taskId);
                if (entity == null)
                {
                    return null;
                }

                // Get existing comments or create new list
                var comments = string.IsNullOrEmpty(entity.CommentsJson)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(entity.CommentsJson);

                // Add new comment
                comments.Add(comment);
                entity.CommentsJson = JsonSerializer.Serialize(comments);

                this._dbContext.Tasks.Update(entity);
                await this._dbContext.SaveChangesAsync();

                return MapEntityToModel(entity);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error adding comment to task with ID {taskId}");
                throw;
            }
        }

        private static Task MapEntityToModel(TaskEntity entity)
        {
            return new Task
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                TodoListId = entity.TodoListId,
                Status = entity.Status,
                DueDate = entity.DueDate,
                CreatedAt = entity.CreatedAt,
                AssignedUserId = entity.AssignedUserId,
                Tags = string.IsNullOrEmpty(entity.TagsJson)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(entity.TagsJson),
                Comments = string.IsNullOrEmpty(entity.CommentsJson)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(entity.CommentsJson)
            };
        }

        public async Task<IEnumerable<Task>> GetFilteredTasksAsync(TaskFilterModel filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            try
            {
                var query = this._dbContext.Tasks.AsQueryable();

                if (filter.TodoListId.HasValue)
                {
                    query = query.Where(t => t.TodoListId == filter.TodoListId.Value);
                }
                if (filter.Status.HasValue)
                {
                    query = query.Where(t => t.Status == filter.Status.Value);
                }
                if (filter.AssignedUserId.HasValue)
                {
                    query = query.Where(t => t.AssignedUserId == filter.AssignedUserId.Value);
                }
                if (filter.DueDateStart.HasValue)
                {
                    query = query.Where(t => t.DueDate >= filter.DueDateStart.Value);
                }
                if (filter.DueDateEnd.HasValue)
                {
                    query = query.Where(t => t.DueDate <= filter.DueDateEnd.Value);
                }
                if (filter.CreatedAtStart.HasValue)
                {
                    query = query.Where(t => t.CreatedAt >= filter.CreatedAtStart.Value);
                }
                if (filter.CreatedAtEnd.HasValue)
                {
                    query = query.Where(t => t.CreatedAt <= filter.CreatedAtEnd.Value);
                }
                var skip = (filter.Page - 1) * filter.PageSize;
                var tasks = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip(skip)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var result = tasks.Select(entity => MapEntityToModel(entity)).ToList();

                if (!string.IsNullOrEmpty(filter.TagFilter))
                {
                    result = result.Where(t =>
                        t.Tags != null &&
                        t.Tags.Any(tag => tag.Contains(filter.TagFilter, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error filtering tasks");
                throw;
            }
        }

        public async Task<Models.Task> ChangeTaskStatusAsync(int taskId, TodoListApp.WebApi.Models.TaskStatus newStatus)
        {
            try
            {
                var entity = await this._dbContext.Tasks.FindAsync(taskId);
                if (entity == null)
                {
                    throw new KeyNotFoundException($"Task with ID {taskId} not found");
                }

                entity.Status = newStatus;

                this._dbContext.Tasks.Update(entity);
                await this._dbContext.SaveChangesAsync();

                return MapEntityToModel(entity);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error changing status for task with ID {taskId}");
                throw;
            }
        }
    }
}
