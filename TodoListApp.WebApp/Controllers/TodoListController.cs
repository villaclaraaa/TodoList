using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApp.Helpers;
using TodoListApp.WebApp.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers;

public class TodoListController : Controller
{
    private readonly ITodoListWebApiService _todoListService;
    private readonly ILogger<TodoListController> _logger;
    private readonly IUserService _userService;
    public TodoListController(
        ITodoListWebApiService service,
        IUserService userService,
        ILogger<TodoListController> logger)
    {
        this._todoListService = service ?? throw new ArgumentNullException(nameof(service));
        this._userService = userService ?? throw new ArgumentNullException(nameof(userService));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid ownerId, int page = 1, int pageSize = 4)
    {
        try
        {
            var todoLitst = await this._todoListService.GetAllTodoListsAsync(ownerId, page, pageSize);
            var totalCount = await this._todoListService.GetTodoListsCountAsync(ownerId);

            var viewModels = todoLitst.Select(Mapper.MapDomainToViewModel).ToList();

            this.ViewBag.TotalCount = totalCount;
            this.ViewBag.PageSize = pageSize;
            this.ViewBag.CurrentPage = page;
            this.ViewBag.OwnerId = ownerId;

            return this.View(viewModels);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting todo lists");
            return this.View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
            throw;
        }
    }

    [HttpGet]
    public IActionResult Create(Guid ownerId)
    {
        return this.View(new TodoListModel { OwnerId = ownerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TodoListModel todoListModel)
    {
        if (!this.ModelState.IsValid)
        {
            foreach (var modelState in this.ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    this._logger.LogWarning($"Validation error: {error.ErrorMessage}");
                }
            }
            return this.View(todoListModel);
        }

        try
        {
            var domainModel = Mapper.MapViewModelToDomain(todoListModel);
            var createdTodoList = await this._todoListService.AddTodoListAsync(domainModel);
            return this.RedirectToAction(nameof(Index), new { ownerId = todoListModel.OwnerId });
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error creating todo list");
            this.ModelState.AddModelError("", "An error occurred while creating the todo list. Please try again.");
            return this.View(todoListModel);
            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var todoList = await this._todoListService.GetTodoListByIdAsync(id);
            if (todoList == null)
            {
                return this.NotFound();
            }

            var viewModel = Mapper.MapDomainToViewModel(todoList);

            return this.View(viewModel);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error retrieving todo list with ID {id} for deletion");
            return this.View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
            throw;
        }
    }

    // US03: Process the deletion of a todo list
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var todoList = await this._todoListService.GetTodoListByIdAsync(id);
            var success = await this._todoListService.DeleteTodoListAsync(id);
            if (!success)
            {
                return this.NotFound();
            }

            return this.RedirectToAction(nameof(Index), new { ownerId = todoList.OwnerId });
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error deleting todo list with ID {id}");
            return this.View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
            throw;
        }
    }

    // US04: Display form to edit a todo list
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var todoList = await this._todoListService.GetTodoListByIdAsync(id);
            if (todoList == null)
            {
                return this.NotFound();
            }

            var viewModel = Mapper.MapDomainToViewModel(todoList);

            return this.View(viewModel);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error retrieving todo list with ID {id} for editing");
            return this.View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
            throw;
        }
    }

    // US04: Process form submission to update a todo list
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TodoListModel todoListModel)
    {
        if (id != todoListModel.Id)
        {
            return this.BadRequest();
        }

        if (!this.ModelState.IsValid)
        {
            return this.View(todoListModel);
        }

        try
        {
            var domainModel = Mapper.MapViewModelToDomain(todoListModel);
            var updatedTodoList = await this._todoListService.UpdateTodoListAsync(id, domainModel);
            return this.RedirectToAction(nameof(Index), new { ownerId = todoListModel.OwnerId });
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error updating todo list with ID {id}");
            this.ModelState.AddModelError("", "An error occurred while updating the todo list. Please try again.");
            return this.View(todoListModel);
            throw;
        }
    }

    // US04: Display details of a todo list
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var todoList = await this._todoListService.GetTodoListByIdAsync(id);
            if (todoList == null)
            {
                return this.NotFound();
            }

            var viewModel = Mapper.MapDomainToViewModel(todoList);
            var userNames = new Dictionary<Guid, string>();
            var userEmails = new Dictionary<Guid, string>();

            foreach (var task in viewModel.Tasks)
            {
                if (task.AssignedUserId != Guid.Empty && !userNames.ContainsKey(task.AssignedUserId))
                {
                    var user = await this._userService.GetUserByIdAsync(task.AssignedUserId);
                    if (user != null)
                    {
                        userNames[task.AssignedUserId] = $"{user.FirstName} {user.LastName}";
                        userEmails[task.AssignedUserId] = user.Email;
                    }
                    else
                    {
                        userNames[task.AssignedUserId] = "Unknown User";
                    }
                }
            }

            this.ViewBag.UserNames = userNames;
            this.ViewBag.UserEmails = userEmails;
            this.ViewBag.CurrentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return this.View(viewModel);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, $"Error retrieving todo list details with ID {id}");
            return this.View("Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
            throw;
        }
    }
}
