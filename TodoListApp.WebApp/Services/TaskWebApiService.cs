using System.Text.Json;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Services;

public class TaskWebApiService : ITaskWebApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public TaskWebApiService(HttpClient httpClient, ILogger<TaskWebApiService> logger)
    {
        this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

        this._jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
    public async Task<TodoTask> AddTaskAsync(TodoTask task)
    {
        try
        {
            var response = await this._httpClient.PostAsJsonAsync("api/Task", task);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TodoTask>(content, this._jsonOptions);
            return result;
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
            var response = await this._httpClient.DeleteAsync($"api/Task/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error deleting task with ID {id}");
            throw;
        }
    }

    public async Task<TodoTask> GetTaskByIdAsync(int id)
    {
        try
        {
            var response = await this._httpClient.GetAsync($"api/Task/{id}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TodoTask>(content, this._jsonOptions);
            return result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error getting task with ID {id}");
            throw;
        }
    }

    public async Task<IEnumerable<TodoTask>> GetTasksByTodoListIdAsync(int todoListId, int page, int pageSize)
    {
        try
        {
            var response = await this._httpClient.GetAsync($"api/Task/todoList/{todoListId}?page={page}&pageSize={pageSize}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<IEnumerable<TodoTask>>(content, this._jsonOptions);
            return result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error getting tasks for todo list ID {todoListId}");
            throw;
        }
    }

    public async Task<TodoTask> UpdateTaskAsync(int id, TodoTask task)
    {
        try
        {
            var response = await this._httpClient.PutAsJsonAsync($"api/Task/{id}", task);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TodoTask>(content, this._jsonOptions);
            return result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error updating task with ID {id}");
            throw;
        }
    }

    public async Task<IEnumerable<TodoTask>> AssignedTasks(Guid userId, int page, int pageSize,
    TaskSortOption sortBy, bool descending, Models.TaskStatus? statusFilter, string? tagFilter = null)
    {
        try
        {
            string queryString = $"api/Task/assigned/{userId}?page={page}&pageSize={pageSize}";

            if (sortBy != TaskSortOption.Default)
            {
                queryString += $"&sortOption={(int)sortBy}&descending={descending}";
            }

            if (statusFilter.HasValue)
            {
                queryString += $"&statusFilter={(int)statusFilter.Value}";
            }

            if (!string.IsNullOrEmpty(tagFilter))
            {
                queryString += $"&tagFilter={Uri.EscapeDataString(tagFilter)}";
            }

            var response = await this._httpClient.GetAsync(queryString);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<IEnumerable<TodoTask>>(content, this._jsonOptions);
            return result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error getting tasks for user with ID {userId}");
            throw;
        }
    }

    public async Task<int> GetAssignedTasksCountAsync(Guid userId,
        Models.TaskStatus? statusFilter, string? tagFilter = null)
    {
        try
        {
            var queryString = $"api/Task/assigned/{userId}/count";
            bool hasParam = false;
            if (statusFilter.HasValue)
            {
                queryString += $"?statusFilter={(int)statusFilter.Value}";
                hasParam = true;
            }

            if (!string.IsNullOrEmpty(tagFilter))
            {
                queryString += hasParam ? $"&tagFilter={Uri.EscapeDataString(tagFilter)}" : $"?tagFilter={Uri.EscapeDataString(tagFilter)}";
            }

            var response = await this._httpClient.GetAsync(queryString);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(content, this._jsonOptions);
            return count;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error getting count of tasks assigned to user {userId}");
            throw;
        }
    }
}
