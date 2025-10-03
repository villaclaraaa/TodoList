using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Services;

public interface ITodoListWebApiService
{
    Task<IEnumerable<TodoList>> GetAllTodoListsAsync(Guid ownerId, int page = 1, int pageSize = 10);
    Task<int> GetTodoListsCountAsync(Guid ownerId);
    Task<TodoList> AddTodoListAsync(TodoList todoList);
    Task<bool> DeleteTodoListAsync(int id);
    Task<TodoList> GetTodoListByIdAsync(int id);
    Task<TodoList> UpdateTodoListAsync(int id, TodoList todoList);
}
