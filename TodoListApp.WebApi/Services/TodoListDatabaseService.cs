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
                Title = todoList.Title,
                Description = todoList.Description,
                CreatedAt = DateTime.UtcNow,
                OwnerId = ownerId,
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
                .Where(t => t.OwnerId == ownerId)
                .OrderByDescending(t => t.CreatedAt)
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
            var entity = await this._dbContext.TodoLists.FindAsync(id);
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
            entity.UpdatedAt = DateTime.Now;

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
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
