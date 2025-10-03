using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApp.Helpers;
using TodoListApp.WebApp.Models;
using TodoListApp.WebApp.Services;
namespace TodoListApp.WebApp.Controllers;

public class TaskController : Controller
{
    private readonly ITaskWebApiService _taskService;
    private readonly ILogger<TaskController> _logger;
    private readonly ITodoListWebApiService _todoListService;

    public TaskController(ITaskWebApiService taskService, ITodoListWebApiService todoListService, ILogger<TaskController> logger)
    {
        this._taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        this._todoListService = todoListService ?? throw new ArgumentNullException(nameof(todoListService));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    public async Task<IActionResult> Create(TodoTaskModel taskModel)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            taskModel.AssignedUserId = 1;
            var domainModel = Mapper.MapTaskViewModelToDomain(taskModel);
            await this._taskService.AddTaskAsync(domainModel);

            return this.RedirectToAction("Details", "TodoList", new { id = taskModel.TodoListId });
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error creating task");
            this.ModelState.AddModelError("", "An error occurred while creating the task. Please try again.");
            return this.View(taskModel);
            throw;
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(TodoTaskModel taskModel, string returnUrl = null)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            var domainModel = Mapper.MapTaskViewModelToDomain(taskModel);
            await this._taskService.UpdateTaskAsync(taskModel.Id, domainModel);

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.RedirectToAction("Details", "TodoList", new { id = taskModel.TodoListId });
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error updating task with ID {taskModel.Id}");
            this.ModelState.AddModelError("", "An error occurred while updating the task. Please try again.");
            return this.View("Error");
            throw;
        }
    }

    [HttpGet("assigned/{userId}")]
    public async Task<IActionResult> AssignedTasks(int userId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] TaskSortOption sortBy = TaskSortOption.Default,
    [FromQuery] bool descending = false,
    [FromQuery] Models.TaskStatus? statusFilter = null)
    {
        try
        {
            var tasks = await this._taskService.AssignedTasks(userId, page, pageSize, sortBy, descending, statusFilter);

            var viewModels = tasks.Select(Mapper.MapTaskDomainToViewModel).ToList();

            var todoListIds = viewModels.Select(t => t.TodoListId).Distinct().ToList();

            var todoListNames = new Dictionary<int, string>();

            foreach (var listId in todoListIds)
            {
                try
                {
                    var todoList = await this._todoListService.GetTodoListByIdAsync(listId);
                    if (todoList != null)
                    {
                        todoListNames[listId] = todoList.Title;
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogWarning(ex, $"Could not retrieve TodoList with ID {listId}");
                    todoListNames[listId] = "Unknown List";
                    throw;
                }
            }
            this.ViewBag.TodoListNames = todoListNames;

            this.ViewBag.CurrentPage = page;
            this.ViewBag.PageSize = pageSize;
            this.ViewBag.UserId = userId;
            this.ViewBag.SortBy = sortBy;
            this.ViewBag.Descending = descending;
            this.ViewBag.StatusFilter = statusFilter;

            this.ViewBag.TotalCount = await this._taskService.GetAssignedTasksCountAsync(userId);
            this.ViewBag.PredefinedTags = TagsService.PredefinedTags;
            return this.View(viewModels);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error retrieving tasks assigned to user {userId}");
            return this.View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
            throw;
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddTag(int taskId, string tag, string returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return this.BadRequest("Tag cannot be empty");
        }

        try
        {
            var task = await this._taskService.GetTaskByIdAsync(taskId);
            if (task == null)
            {
                return this.NotFound();
            }

            if (task.Tags == null)
            {
                task.Tags = new List<string>();
            }

            if (!task.Tags.Contains(tag))
            {
                task.Tags.Add(tag);

                await this._taskService.UpdateTaskAsync(taskId, task);
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.RedirectToAction("Details", "TodoList", new { id = task.TodoListId });
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error adding tag to task with ID {taskId}");
            this.ModelState.AddModelError("", "An error occurred while adding the tag.");
            return this.View("Error");
            throw;
        }
    }

    [HttpPost]
    public async Task<IActionResult> RemoveTag(int taskId, string tag, string returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return this.BadRequest("Tag cannot be empty");
        }

        try
        {
            var task = await this._taskService.GetTaskByIdAsync(taskId);
            if (task == null)
            {
                return this.NotFound();
            }

            if (task.Tags != null && task.Tags.Contains(tag))
            {
                task.Tags.Remove(tag);
                await this._taskService.UpdateTaskAsync(taskId, task);
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.RedirectToAction("Details", "TodoList", new { id = task.TodoListId });
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error removing tag from task with ID {taskId}");
            this.ModelState.AddModelError("", "An error occurred while removing the tag.");
            return this.View("Error");
            throw;
        }
    }
}
