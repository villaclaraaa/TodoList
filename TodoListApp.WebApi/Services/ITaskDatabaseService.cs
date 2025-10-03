using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Services
{
    public interface ITaskDatabaseService
    {
        // Get all tasks in a todo list
        Task<IEnumerable<Models.Task>> GetTasksByTodoListIdAsync(int todoListId, int page = 1, int pageSize = 10);

        // Get total count of tasks in a todo list for pagination
        Task<int> GetTasksCountByTodoListIdAsync(int todoListId);

        // Get task details by ID
        Task<Models.Task> GetTaskByIdAsync(int id);

        // Add a new task to a todo list
        Task<Models.Task> AddTaskAsync(Models.Task task);

        // Delete a task
        Task<bool> DeleteTaskAsync(int id);

        // Update an existing task
        Task<Models.Task> UpdateTaskAsync(int id, Models.Task task);

        // Get overdue tasks in a todo list
        Task<IEnumerable<Models.Task>> GetOverdueTasksByTodoListIdAsync(int todoListId, int page = 1, int pageSize = 10);
        Task<Models.Task> AddTagToTaskAsync(int taskId, string tag);
        Task<Models.Task> RemoveTagFromTaskAsync(int taskId, string tag);
        Task<Models.Task> AddCommentToTaskAsync(int taskId, string comment);

        Task<IEnumerable<Models.Task>> GetFilteredTasksAsync(TaskFilterModel filter);
        Task<Models.Task> ChangeTaskStatusAsync(int taskId, TodoListApp.WebApi.Models.TaskStatus newStatus);

        Task<IEnumerable<Models.Task>> GetTasksAssignedToUser(int userId, int page = 1, int pageSize = 10,
            TaskSortOption sortOption = TaskSortOption.Default, bool descending = false,
            Models.TaskStatus? statusFilter = null);
        Task<int> GetAssignedTasksCount(int userId, Models.TaskStatus? statusFilter = null);
    }
}
