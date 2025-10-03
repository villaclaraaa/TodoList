using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Services;

public interface ITaskWebApiService
{
    Task<IEnumerable<TodoTask>> GetTasksByTodoListIdAsync(int todoListId, int page = 1, int pageSize = 10);
    Task<TodoTask> GetTaskByIdAsync(int id);
    Task<TodoTask> AddTaskAsync(TodoTask task);
    Task<bool> DeleteTaskAsync(int id);
    Task<TodoTask> UpdateTaskAsync(int id, TodoTask task);

    Task<IEnumerable<TodoTask>> AssignedTasks(int userId, int page = 1, int pageSize = 10,
        TaskSortOption sortBy = TaskSortOption.Default, bool descending = false,
        Models.TaskStatus? statusFilter = null);
    Task<int> GetAssignedTasksCountAsync(int userId, Models.TaskStatus? statusFilter = null);
}
