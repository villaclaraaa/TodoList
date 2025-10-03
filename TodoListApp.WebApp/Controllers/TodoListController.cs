using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApp.Helpers;
using TodoListApp.WebApp.Models;
using TodoListApp.WebApp.Services;
namespace TodoListApp.WebApp.Controllers;

public class TodoListController : Controller
{
    private readonly ITodoListWebApiService _todoListService;
    private readonly ILogger<TodoListController> _logger;
    private readonly int DefaultOwnerId = 1;
    public TodoListController(ITodoListWebApiService service, ILogger<TodoListController> logger)
    {
        this._todoListService = service ?? throw new ArgumentNullException(nameof(service));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        try
        {
            var todoLitst = await this._todoListService.GetAllTodoListsAsync(1, page, pageSize);
            var totalCount = await this._todoListService.GetTodoListsCountAsync(1);

            var viewModels = todoLitst.Select(Mapper.MapDomainToViewModel).ToList();

            this.ViewBag.TotalCount = totalCount;
            this.ViewBag.PageSize = pageSize;
            this.ViewBag.CurrentPage = page;

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
    public IActionResult Create()
    {
        return this.View(new TodoListModel { OwnerId = this.DefaultOwnerId });
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
            todoListModel.OwnerId = this.DefaultOwnerId;
            var domainModel = Mapper.MapViewModelToDomain(todoListModel);
            var createdTodoList = await this._todoListService.AddTodoListAsync(domainModel);
            return this.RedirectToAction(nameof(Index));
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
            var success = await this._todoListService.DeleteTodoListAsync(id);
            if (!success)
            {
                return this.NotFound();
            }

            return this.RedirectToAction(nameof(Index));
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
            return this.RedirectToAction(nameof(Index));
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
