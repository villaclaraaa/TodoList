using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        private readonly ITodoListDatabaseService _todoListService;
        private readonly ILogger<TodoListController> _logger;

        public TodoListController(ITodoListDatabaseService todoListService, ILogger<TodoListController> logger)
        {
            this._todoListService = todoListService ?? throw new ArgumentNullException(nameof(todoListService));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // US01: Get all todo lists
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TodoListModel>>> GetTodoLists([FromQuery] int ownerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var todoLists = await this._todoListService.GetAllTodoListsAsync(ownerId, page, pageSize);
                var count = await this._todoListService.GetTodoListsCountAsync(ownerId);

                var todoListModels = new List<TodoListModel>();
                foreach (var todoList in todoLists)
                {
                    todoListModels.Add(new TodoListModel
                    {
                        Id = todoList.Id,
                        Title = todoList.Title,
                        Description = todoList.Description,
                        OwnerId = ownerId,
                        Tasks = todoList.Tasks.ToTaskModelList().ToList()
                    });
                }

                this.Response.Headers.Add("X-Total-Count", count.ToString());
                this.Response.Headers.Add("X-Page-Size", pageSize.ToString());
                this.Response.Headers.Add("X-Current-Page", page.ToString());

                return this.Ok(todoListModels);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error retrieving todo lists");
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving todo lists");
                throw;
            }
        }

        // US02: Add a new todo list
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TodoListModel>> CreateTodoList([FromBody] TodoListModel todoListModel)
        {
            if (todoListModel == null)
            {
                return this.BadRequest("Todo list data is required");
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                var todoList = new TodoList
                {
                    Title = todoListModel.Title,
                    Description = todoListModel.Description,
                    OwnerId = todoListModel.OwnerId,
                    Tasks = todoListModel.Tasks.ToTaskList().ToList(),
                };

                var createdTodoList = await this._todoListService.AddTodoListAsync(todoList.OwnerId, todoList);

                var createdTodoListModel = new TodoListModel
                {
                    Id = createdTodoList.Id,
                    Title = createdTodoList.Title,
                    Description = createdTodoList.Description,
                };

                return this.CreatedAtAction(nameof(GetTodoList), new { id = createdTodoListModel.Id }, createdTodoListModel);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error creating todo list");
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Error creating todo list");
                throw;
            }
        }

        // US03: Delete a todo list
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTodoList(int id)
        {
            try
            {
                var success = await this._todoListService.DeleteTodoListAsync(id);
                if (!success)
                {
                    return this.NotFound($"Todo list with ID {id} not found");
                }

                return this.NoContent();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error deleting todo list with ID {id}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting todo list with ID {id}");
                throw;
            }
        }

        // US04: Get a specific todo list by ID (used by the update operation)
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TodoListModel>> GetTodoList(int id)
        {
            try
            {
                var todoList = await this._todoListService.GetTodoListByIdAsync(id);
                if (todoList == null)
                {
                    return this.NotFound($"Todo list with ID {id} not found");
                }

                var todoListModel = new TodoListModel
                {
                    Id = todoList.Id,
                    Title = todoList.Title,
                    Description = todoList.Description,
                    OwnerId = todoList.OwnerId,
                    Tasks = todoList.Tasks.ToTaskModelList().ToList()
                };

                return this.Ok(todoListModel);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error retrieving todo list with ID {id}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving todo list with ID {id}");
                throw;
            }
        }

        // US04: Update an existing todo list
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TodoListModel>> UpdateTodoList(int id, [FromBody] TodoListModel todoListModel)
        {
            if (todoListModel == null)
            {
                return this.BadRequest("Todo list data is required");
            }

            if (id != todoListModel.Id)
            {
                return this.BadRequest("ID mismatch");
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                var todoList = new TodoList
                {
                    Id = todoListModel.Id,
                    Title = todoListModel.Title,
                    Description = todoListModel.Description,
                    OwnerId = todoListModel.OwnerId,
                    Tasks = todoListModel.Tasks.ToTaskList().ToList(),
                };

                var updatedTodoList = await this._todoListService.UpdateTodoListAsync(id, todoList);
                if (updatedTodoList == null)
                {
                    return this.NotFound($"Todo list with ID {id} not found");
                }

                var updatedTodoListModel = new TodoListModel
                {
                    Id = updatedTodoList.Id,
                    Title = updatedTodoList.Title,
                    Description = updatedTodoList.Description,
                    OwnerId = todoListModel.OwnerId,
                    Tasks = updatedTodoList.Tasks.ToTaskModelList().ToList()
                };

                return this.Ok(updatedTodoListModel);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error updating todo list with ID {id}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error updating todo list with ID {id}");
                throw;
            }
        }
    }
}

/// <summary>
/// Helper methods to convert between Models.Task and TaskModel
/// </summary>
public static class TaskConverter
{
    /// <summary>
    /// Converts a Models.Task object to a TaskModel object
    /// </summary>
    /// <param name="task">The Models.Task object to convert</param>
    /// <returns>A new TaskModel with properties copied from the task</returns>
    public static TaskModel ToTaskModel(this TodoListApp.WebApi.Models.Task task)
    {
        if (task == null)
        {
            return null;
        }

        return new TaskModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            TodoListId = task.TodoListId,
            Status = task.Status,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            AssignedUserId = task.AssignedUserId,
            Tags = task.Tags?.ToList() ?? new List<string>(),
            Comments = task.Comments?.ToList() ?? new List<string>()
        };
    }

    /// <summary>
    /// Converts a TaskModel object to a Models.Task object
    /// </summary>
    /// <param name="taskModel">The TaskModel object to convert</param>
    /// <returns>A new Models.Task with properties copied from the taskModel</returns>
    public static TodoListApp.WebApi.Models.Task ToTask(this TaskModel taskModel)
    {
        if (taskModel == null)
        {
            return null;
        }

        return new TodoListApp.WebApi.Models.Task
        {
            Id = taskModel.Id,
            Title = taskModel.Title,
            Description = taskModel.Description,
            TodoListId = taskModel.TodoListId,
            Status = taskModel.Status,
            DueDate = taskModel.DueDate,
            CreatedAt = taskModel.CreatedAt,
            AssignedUserId = taskModel.AssignedUserId,
            Tags = taskModel.Tags?.ToList() ?? new List<string>(),
            Comments = taskModel.Comments?.ToList() ?? new List<string>()
        };
    }

    /// <summary>
    /// Converts a collection of Models.Task objects to a collection of TaskModel objects
    /// </summary>
    /// <param name="tasks">The collection of Models.Task objects to convert</param>
    /// <returns>A list of TaskModel objects</returns>
    public static IEnumerable<TaskModel> ToTaskModelList(this IEnumerable<TodoListApp.WebApi.Models.Task> tasks)
    {
        return tasks?.Select(t => t.ToTaskModel()).ToList() ?? new List<TaskModel>();
    }

    /// <summary>
    /// Converts a collection of TaskModel objects to a collection of Models.Task objects
    /// </summary>
    /// <param name="taskModels">The collection of TaskModel objects to convert</param>
    /// <returns>A list of Models.Task objects</returns>
    public static IEnumerable<TodoListApp.WebApi.Models.Task> ToTaskList(this IEnumerable<TaskModel> taskModels)
    {
        return taskModels?.Select(tm => tm.ToTask()).ToList() ?? new List<TodoListApp.WebApi.Models.Task>();
    }
}
