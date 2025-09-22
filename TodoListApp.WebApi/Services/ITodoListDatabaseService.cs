using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Services
{
    public interface ITodoListDatabaseService
    {
        // US01: Get all todo lists
        Task<IEnumerable<TodoList>> GetAllTodoListsAsync(int ownerId, int page = 1, int pageSize = 10);

        // US01: Get total count for pagination
        Task<int> GetTodoListsCountAsync(int ownerId);

        // US02: Add a new todo list
        Task<TodoList> AddTodoListAsync(int ownerId, TodoList todoList);

        // US03: Delete a todo list
        Task<bool> DeleteTodoListAsync(int id);

        // US04: Get a specific todo list by ID
        Task<TodoList> GetTodoListByIdAsync(int id);

        // US04: Update an existing todo list
        Task<TodoList> UpdateTodoListAsync(int id, TodoList todoList);
    }
}
