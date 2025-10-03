using System.Text;
using System.Text.Json;
using TodoListApp.WebApp.Helpers;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Services;

public class TodoListWebApiService : ITodoListWebApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TodoListWebApiService> _logger;
    private readonly string _baseUrl = "api/TodoList";

    public TodoListWebApiService(HttpClient httpClient, ILogger<TodoListWebApiService> logger)
    {
        this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // US01: Get all todo lists
    public async Task<IEnumerable<TodoList>> GetAllTodoListsAsync(int ownerId, int page = 1, int pageSize = 10)
    {
        try
        {
            var response = await this._httpClient.GetAsync($"{this._baseUrl}?ownerId={ownerId}&page={page}&pageSize={pageSize}");
            response.EnsureSuccessStatusCode();

            var todoLists = await response.Content.ReadFromJsonAsync<IEnumerable<TodoListWebApiModel>>();
            return todoLists?.Select(Mapper.MapApiModelToDomain) ?? new List<TodoList>();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving todo lists");
            throw;
        }
    }

    // US01: Get total count for pagination
    public async Task<int> GetTodoListsCountAsync(int ownerId)
    {
        try
        {
            var response = await this._httpClient.GetAsync($"{this._baseUrl}?ownerId={ownerId}&page=1&pageSize=1");
            response.EnsureSuccessStatusCode();

            if (response.Headers.TryGetValues("X-Total-Count", out var values))
            {
                if (int.TryParse(values.FirstOrDefault(), out int count))
                {
                    return count;
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving todo list count");
            throw;
        }
    }

    // US02: Add a new todo list
    public async Task<TodoList> AddTodoListAsync(TodoList todoList)
    {
        try
        {
            var apiModel = Mapper.MapDomainToApiModel(todoList);
            var content = new StringContent(
                JsonSerializer.Serialize(apiModel),
                Encoding.UTF8,
                "application/json");

            var response = await this._httpClient.PostAsync($"{this._baseUrl}", content);
            response.EnsureSuccessStatusCode();

            var createdTodoList = await response.Content.ReadFromJsonAsync<TodoListWebApiModel>();
            return Mapper.MapApiModelToDomain(createdTodoList);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error creating todo list");
            throw;
        }
    }

    // US03: Delete a todo list
    public async Task<bool> DeleteTodoListAsync(int id)
    {
        try
        {
            var response = await this._httpClient.DeleteAsync($"{this._baseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error deleting todo list with ID {id}");
            throw;
        }
    }

    // US04: Get a specific todo list by ID
    public async Task<TodoList> GetTodoListByIdAsync(int id)
    {
        try
        {
            var response = await this._httpClient.GetAsync($"{this._baseUrl}/{id}");
            response.EnsureSuccessStatusCode();

            var todoList = await response.Content.ReadFromJsonAsync<TodoListWebApiModel>();
            return Mapper.MapApiModelToDomain(todoList);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error retrieving todo list with ID {id}");
            throw;
        }
    }

    // US04: Update an existing todo list
    public async Task<TodoList> UpdateTodoListAsync(int id, TodoList todoList)
    {
        try
        {
            var apiModel = Mapper.MapDomainToApiModel(todoList);
            var content = new StringContent(
                JsonSerializer.Serialize(apiModel),
                Encoding.UTF8,
                "application/json");

            var response = await this._httpClient.PutAsync($"{this._baseUrl}/{id}", content);
            response.EnsureSuccessStatusCode();

            var updatedTodoList = await response.Content.ReadFromJsonAsync<TodoListWebApiModel>();
            return Mapper.MapApiModelToDomain(updatedTodoList);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error updating todo list with ID {id}");
            throw;
        }
    }
}
