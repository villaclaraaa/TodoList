using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Services;

public interface ITodoListWebApiService
{
    Task<IEnumerable<TodoListWebApiModel>> GetAllTodoListsAsync(int ownerId, int page = 1, int pageSize = 10);
    Task<int> GetTodoListsCountAsync(int ownerId);
    Task<TodoListWebApiModel> AddTodoListAsync(TodoListWebApiModel todoList);
    Task<bool> DeleteTodoListAsync(int id);
    Task<TodoListWebApiModel> GetTodoListByIdAsync(int id);
    Task<TodoListWebApiModel> UpdateTodoListAsync(int id, TodoListWebApiModel todoList);

}
