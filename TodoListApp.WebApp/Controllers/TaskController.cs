using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApp.Helpers;
using TodoListApp.WebApp.Models;
using TodoListApp.WebApp.Services;
namespace TodoListApp.WebApp.Controllers;

public class TaskController : Controller
{
    private readonly ITaskWebApiService _taskService;
    private readonly ILogger<TaskController> _logger;

    public TaskController(ITaskWebApiService taskService, ILogger<TaskController> logger)
    {
        this._taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
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
    public async Task<IActionResult> Update(TodoTaskModel taskModel)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            var domainModel = Mapper.MapTaskViewModelToDomain(taskModel);
            await this._taskService.UpdateTaskAsync(taskModel.Id, domainModel);

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
    [FromQuery] bool descending = false)
    {
        try
        {
            var tasks = await this._taskService.AssignedTasks(userId, page, pageSize, sortBy, descending);

            var viewModels = tasks.Select(Mapper.MapTaskDomainToViewModel).ToList();

            this.ViewBag.CurrentPage = page;
            this.ViewBag.PageSize = pageSize;
            this.ViewBag.UserId = userId;
            this.ViewBag.SortBy = sortBy;
            this.ViewBag.Descending = descending;

            this.ViewBag.TotalCount = await this._taskService.GetAssignedTasksCountAsync(userId);

            return this.View(viewModels);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error retrieving tasks assigned to user {userId}");
            return this.View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
            throw;
        }
    }
}
