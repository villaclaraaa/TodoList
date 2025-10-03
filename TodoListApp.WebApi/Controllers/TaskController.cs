using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : Controller
    {
        private readonly ITaskDatabaseService _taskService;
        private readonly ITodoListDatabaseService _todoListDatabaseService;
        private readonly ILogger<TaskController> _logger;

        public TaskController(ITaskDatabaseService taskService, ILogger<TaskController> logger, ITodoListDatabaseService todoListDatabaseService)
        {
            this._taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._todoListDatabaseService = todoListDatabaseService;
        }

        // US05: Get all tasks in a todo list
        [HttpGet("todolist/{todoListId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TaskModel>>> GetTasksByTodoListId(
            int todoListId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Check if the todo list exists
                var todoList = await this._todoListDatabaseService.GetTodoListByIdAsync(todoListId);
                if (todoList == null)
                {
                    return this.NotFound($"Todo list with ID {todoListId} not found");
                }

                var tasks = await this._taskService.GetTasksByTodoListIdAsync(todoListId, page, pageSize);
                var count = await this._taskService.GetTasksCountByTodoListIdAsync(todoListId);

                var taskModels = new List<TaskModel>();
                foreach (var task in tasks)
                {
                    taskModels.Add(new TaskModel
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Description = task.Description,
                        TodoListId = task.TodoListId,
                        Status = task.Status,
                        DueDate = task.DueDate,
                        CreatedAt = task.CreatedAt,
                        AssignedUserId = task.AssignedUserId,
                        Tags = task.Tags,
                        Comments = task.Comments
                    });
                }

                this.Response.Headers.Add("X-Total-Count", count.ToString());
                this.Response.Headers.Add("X-Page-Size", pageSize.ToString());
                this.Response.Headers.Add("X-Current-Page", page.ToString());

                return this.Ok(taskModels);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error retrieving tasks for todo list ID {todoListId}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving tasks for todo list ID {todoListId}");
                throw;
            }
        }

        // Get overdue tasks in a todo list
        [HttpGet("todolist/{todoListId}/overdue")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TaskModel>>> GetOverdueTasksByTodoListId(
            int todoListId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Check if the todo list exists
                var todoList = await this._todoListDatabaseService.GetTodoListByIdAsync(todoListId);
                if (todoList == null)
                {
                    return this.NotFound($"Todo list with ID {todoListId} not found");
                }

                var tasks = await this._taskService.GetOverdueTasksByTodoListIdAsync(todoListId, page, pageSize);

                var taskModels = new List<TaskModel>();
                foreach (var task in tasks)
                {
                    taskModels.Add(new TaskModel
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Description = task.Description,
                        TodoListId = task.TodoListId,
                        Status = task.Status,
                        DueDate = task.DueDate,
                        CreatedAt = task.CreatedAt,
                        AssignedUserId = task.AssignedUserId,
                        Tags = task.Tags,
                        Comments = task.Comments
                    });
                }

                return this.Ok(taskModels);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error retrieving overdue tasks for todo list ID {todoListId}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving overdue tasks for todo list ID {todoListId}");
                throw;
            }
        }

        // US06: Get task details by ID
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskModel>> GetTask(int id)
        {
            try
            {
                var task = await this._taskService.GetTaskByIdAsync(id);
                if (task == null)
                {
                    return this.NotFound($"Task with ID {id} not found");
                }

                var taskModel = new TaskModel
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    TodoListId = task.TodoListId,
                    Status = task.Status,
                    DueDate = task.DueDate,
                    CreatedAt = task.CreatedAt,
                    AssignedUserId = task.AssignedUserId,
                    Tags = task.Tags,
                    Comments = task.Comments
                };

                return this.Ok(taskModel);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error retrieving task with ID {id}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving task with ID {id}");
                throw;
            }
        }

        // Add a new task
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskModel>> CreateTask([FromBody] TaskModel taskModel)
        {
            if (taskModel == null)
            {
                return this.BadRequest("Task data is required");
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                // Check if the todo list exists
                var todoList = await this._todoListDatabaseService.GetTodoListByIdAsync(taskModel.TodoListId);
                if (todoList == null)
                {
                    return this.BadRequest($"Todo list with ID {taskModel.TodoListId} not found");
                }

                var task = new Models.Task
                {
                    Title = taskModel.Title,
                    Description = taskModel.Description,
                    TodoListId = taskModel.TodoListId,
                    Status = taskModel.Status,
                    DueDate = taskModel.DueDate,
                    AssignedUserId = taskModel.AssignedUserId,
                    Tags = taskModel.Tags,
                    Comments = taskModel.Comments
                };

                var createdTask = await this._taskService.AddTaskAsync(task);

                var createdTaskModel = new TaskModel
                {
                    Id = createdTask.Id,
                    Title = createdTask.Title,
                    Description = createdTask.Description,
                    TodoListId = createdTask.TodoListId,
                    Status = createdTask.Status,
                    DueDate = createdTask.DueDate,
                    CreatedAt = createdTask.CreatedAt,
                    AssignedUserId = createdTask.AssignedUserId,
                    Tags = createdTask.Tags,
                    Comments = createdTask.Comments
                };

                return this.CreatedAtAction(nameof(GetTask), new { id = createdTaskModel.Id }, createdTaskModel);
            }
            catch (InvalidOperationException ex)
            {
                return this.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error creating task");
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Error creating task");
                throw;
            }
        }

        // Delete a task
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var success = await this._taskService.DeleteTaskAsync(id);
                if (!success)
                {
                    return this.NotFound($"Task with ID {id} not found");
                }

                return this.NoContent();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error deleting task with ID {id}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting task with ID {id}");
                throw;
            }
        }

        // Update an existing task
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskModel>> UpdateTask(int id, [FromBody] TaskModel taskModel)
        {
            if (taskModel == null)
            {
                return this.BadRequest("Task data is required");
            }

            if (id != taskModel.Id)
            {
                return this.BadRequest("ID mismatch");
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                var task = new Models.Task
                {
                    Id = taskModel.Id,
                    Title = taskModel.Title,
                    Description = taskModel.Description,
                    TodoListId = taskModel.TodoListId,
                    Status = taskModel.Status,
                    DueDate = taskModel.DueDate,
                    AssignedUserId = taskModel.AssignedUserId,
                    Tags = taskModel.Tags,
                    Comments = taskModel.Comments
                };

                var updatedTask = await this._taskService.UpdateTaskAsync(id, task);
                if (updatedTask == null)
                {
                    return this.NotFound($"Task with ID {id} not found");
                }

                var updatedTaskModel = new TaskModel
                {
                    Id = updatedTask.Id,
                    Title = updatedTask.Title,
                    Description = updatedTask.Description,
                    TodoListId = updatedTask.TodoListId,
                    Status = updatedTask.Status,
                    DueDate = updatedTask.DueDate,
                    CreatedAt = updatedTask.CreatedAt,
                    AssignedUserId = updatedTask.AssignedUserId,
                    Tags = updatedTask.Tags,
                    Comments = updatedTask.Comments
                };

                return this.Ok(updatedTaskModel);
            }
            catch (InvalidOperationException ex)
            {
                return this.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error updating task with ID {id}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error updating task with ID {id}");
                throw;
            }
        }

        [HttpPost("{id}/tags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskModel>> AddTagToTask(int id, [FromBody] string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return this.BadRequest("Tag is required");
            }

            try
            {
                var updatedTask = await this._taskService.AddTagToTaskAsync(id, tag);
                if (updatedTask == null)
                {
                    return this.NotFound($"Task with ID {id} not found");
                }

                var taskModel = new TaskModel
                {
                    Id = updatedTask.Id,
                    Title = updatedTask.Title,
                    Description = updatedTask.Description,
                    TodoListId = updatedTask.TodoListId,
                    Status = updatedTask.Status,
                    DueDate = updatedTask.DueDate,
                    CreatedAt = updatedTask.CreatedAt,
                    AssignedUserId = updatedTask.AssignedUserId,
                    Tags = updatedTask.Tags,
                    Comments = updatedTask.Comments
                };

                return this.Ok(taskModel);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error adding tag to task with ID {id}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error adding tag to task");
                throw;
            }
        }

        [HttpPost("{id}/comments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskModel>> AddCommentToTask(int id, [FromBody] string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return this.BadRequest("Comment is required");
            }

            try
            {
                var updatedTask = await this._taskService.AddCommentToTaskAsync(id, comment);
                if (updatedTask == null)
                {
                    return this.NotFound($"Task with ID {id} not found");
                }

                var taskModel = new TaskModel
                {
                    Id = updatedTask.Id,
                    Title = updatedTask.Title,
                    Description = updatedTask.Description,
                    TodoListId = updatedTask.TodoListId,
                    Status = updatedTask.Status,
                    DueDate = updatedTask.DueDate,
                    CreatedAt = updatedTask.CreatedAt,
                    AssignedUserId = updatedTask.AssignedUserId,
                    Tags = updatedTask.Tags,
                    Comments = updatedTask.Comments
                };

                return this.Ok(taskModel);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error adding comment to task with ID {id}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error adding comment to task");
                throw;
            }
        }

        [HttpPost("filter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TaskModel>>> PostFilteredTasks([FromBody] TaskFilterModel filter)
        {
            if (filter == null)
            {
                return this.BadRequest("Filter criteria are required");
            }

            try
            {
                // Validate date ranges if provided
                if (filter.DueDateStart.HasValue && filter.DueDateEnd.HasValue && filter.DueDateStart > filter.DueDateEnd)
                {
                    return this.BadRequest("Due date start must be before or equal to due date end");
                }

                if (filter.CreatedAtStart.HasValue && filter.CreatedAtEnd.HasValue && filter.CreatedAtStart > filter.CreatedAtEnd)
                {
                    return this.BadRequest("Created at start must be before or equal to created at end");
                }

                // Get filtered tasks and count
                var tasks = await this._taskService.GetFilteredTasksAsync(filter);

                // Map to response models
                var taskModels = tasks.Select(task => new TaskModel
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    TodoListId = task.TodoListId,
                    Status = task.Status,
                    DueDate = task.DueDate,
                    CreatedAt = task.CreatedAt,
                    AssignedUserId = task.AssignedUserId,
                    Tags = task.Tags,
                    Comments = task.Comments
                }).ToList();

                return this.Ok(taskModels);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error filtering tasks");
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Error filtering tasks");
                throw;
            }
        }

        [HttpGet("assigned/{userId}")]
        public async Task<ActionResult<IEnumerable<TaskModel>>> GetTasksAssignedToUser(int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] TaskSortOption sortOption = TaskSortOption.Default,
            [FromQuery] bool descending = false,
            [FromQuery] Models.TaskStatus? statusFilter = null)
        {
            try
            {
                var tasksList = await this._taskService.GetTasksAssignedToUser(userId, page, pageSize, sortOption, descending, statusFilter);
                if (tasksList == null)
                {
                    return this.NotFound($"Tasks for user with {userId} not found");
                }

                var taskModels = new List<TaskModel>();
                foreach (var task in tasksList)
                {
                    taskModels.Add(new TaskModel
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Description = task.Description,
                        TodoListId = task.TodoListId,
                        Status = task.Status,
                        DueDate = task.DueDate,
                        CreatedAt = task.CreatedAt,
                        AssignedUserId = task.AssignedUserId,
                        Tags = task.Tags,
                        Comments = task.Comments
                    });
                }

                this.Response.Headers.Add("X-Total-Count", tasksList.Count().ToString());
                this.Response.Headers.Add("X-Page-Size", pageSize.ToString());
                this.Response.Headers.Add("X-Current-Page", page.ToString());

                return this.Ok(taskModels);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error retrieving tasks for user with ID {userId}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving tasks for user with ID {userId}");
                throw;
            }
        }

        [HttpGet("assigned/{userId}/count")]
        public async Task<ActionResult<int>> GetAssignedTasksCount(int userId,
            [FromQuery] Models.TaskStatus? statusFilter = null)
        {
            try
            {
                var count = await this._taskService.GetAssignedTasksCount(userId, statusFilter);

                return this.Ok(count);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error retrieving tasks count for user with ID {userId}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving tasks count for user with ID {userId}");
                throw;
            }
        }

    }
}
