using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Contexts;
using TodoListApp.Services.Database.Entities;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Services;
public class TodoListDatabaseService : ITodoListDatabaseService
{
    private readonly ILogger<TodoListDatabaseService> _logger;
    private readonly TodoListDbContext _dbContext;

    public TodoListDatabaseService(TodoListDbContext dbContext, ILogger<TodoListDatabaseService> logger)
    {
        this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TodoList> AddTodoListAsync(int ownerId, TodoList todoList)
    {
        ArgumentNullException.ThrowIfNull(todoList);

        try
        {
            var entity = new TodoListEntity
            {
                Id = todoList.Id,
                Title = todoList.Title,
                Description = todoList.Description,
                OwnerId = ownerId,
                Tasks = MapTaskModelsToEntities(todoList.Tasks),
            };

            await this._dbContext.TodoLists.AddAsync(entity);
            await this._dbContext.SaveChangesAsync();

            return MapEntityToModel(entity);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error adding todo list");
            throw;
        }

    }

    public async Task<bool> DeleteTodoListAsync(int id)
    {
        try
        {
            var entity = await this._dbContext.TodoLists.FindAsync(id);

            if (entity == null)
            {
                return false;
            }

            this._dbContext.TodoLists.Remove(entity);
            await this._dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error deleting todo list with ID {id}");
            throw;
        }
    }

    public async Task<IEnumerable<TodoList>> GetAllTodoListsAsync(int ownerId, int page = 1, int pageSize = 10)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            var todoLists = await this._dbContext.TodoLists
                .Include(t => t.Tasks)
                .Where(t => t.OwnerId == ownerId)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return todoLists.Select(entity => MapEntityToModel(entity));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving todo lists");
            throw;
        }
    }

    public async Task<TodoList?> GetTodoListByIdAsync(int id)
    {
        try
        {
            var entity = await this._dbContext.TodoLists.Include(t => t.Tasks).FirstOrDefaultAsync(t => t.Id == id);
            return entity != null ? MapEntityToModel(entity) : null;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error retrieving todo list with ID {id}");
            throw;
        }
    }

    public async Task<int> GetTodoListsCountAsync(int ownerId)
    {
        try
        {
            return await this._dbContext.TodoLists.CountAsync(t => t.OwnerId == ownerId);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error counting todo lists");
            throw;
        }
    }

    public async Task<TodoList> UpdateTodoListAsync(int id, TodoList todoList)
    {
        ArgumentNullException.ThrowIfNull(todoList);
        try
        {
            var entity = await this._dbContext.TodoLists.FindAsync(id);

            if (entity == null)
            {
                return null;
            }

            entity.Title = todoList.Title;
            entity.Description = todoList.Description;

            this._dbContext.TodoLists.Update(entity);
            await this._dbContext.SaveChangesAsync();

            return MapEntityToModel(entity);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error updating todo list with ID {id}");
            throw;
        }
    }

    private static TodoList MapEntityToModel(TodoListEntity entity)
    {
        return new TodoList
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            OwnerId = entity.OwnerId,
            Tasks = MapTaskEntitiesToModels(entity.Tasks)
        };
    }
    private static Models.Task MapTaskEntityToModel(TaskEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        // Parse JSON strings to lists
        List<string> tags = new List<string>();
        List<string> comments = new List<string>();

        if (!string.IsNullOrEmpty(entity.TagsJson))
        {
            tags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.TagsJson);
        }

        if (!string.IsNullOrEmpty(entity.CommentsJson))
        {
            comments = System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.CommentsJson);
        }

        return new Models.Task
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            TodoListId = entity.TodoListId,
            Status = entity.Status,
            DueDate = entity.DueDate,
            CreatedAt = entity.CreatedAt,
            AssignedUserId = entity.AssignedUserId,
            Tags = tags ?? new List<string>(),
            Comments = comments ?? new List<string>()
        };
    }

    private static TaskEntity MapTaskModelToEntity(Models.Task task)
    {
        if (task == null)
        {
            return null;
        }

        // Serialize lists to JSON
        string tagsJson = null;
        string commentsJson = null;

        if (task.Tags?.Any() == true)
        {
            tagsJson = System.Text.Json.JsonSerializer.Serialize(task.Tags);
        }

        if (task.Comments?.Any() == true)
        {
            commentsJson = System.Text.Json.JsonSerializer.Serialize(task.Comments);
        }

        return new TaskEntity
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            TodoListId = task.TodoListId,
            Status = task.Status,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            AssignedUserId = task.AssignedUserId,
            TagsJson = tagsJson,
            CommentsJson = commentsJson
        };
    }

    private static ICollection<Models.Task> MapTaskEntitiesToModels(ICollection<TaskEntity> entities)
    {
        return entities?.Select(MapTaskEntityToModel).ToList() ?? new List<Models.Task>();
    }

    private static ICollection<TaskEntity> MapTaskModelsToEntities(ICollection<Models.Task> tasks)
    {
        return tasks?.Select(MapTaskModelToEntity).ToList() ?? new List<TaskEntity>();
    }
}
