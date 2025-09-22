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
                var todoLists = await this._todoListService.GetAllTodoListsAsync(page, pageSize);
                var count = await this._todoListService.GetTodoListsCountAsync(ownerId);

                var todoListModels = new List<TodoListModel>();
                foreach (var todoList in todoLists)
                {
                    todoListModels.Add(new TodoListModel
                    {
                        Id = todoList.Id,
                        Title = todoList.Title,
                        Description = todoList.Description,
                        CreatedAt = todoList.CreatedAt,
                        UpdatedAt = todoList.UpdatedAt
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
        [HttpPost("{id}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TodoListModel>> CreateTodoList([FromQuery] int ownerId, [FromBody] TodoListModel todoListModel)
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
                    Description = todoListModel.Description
                };

                var createdTodoList = await this._todoListService.AddTodoListAsync(ownerId, todoList);

                var createdTodoListModel = new TodoListModel
                {
                    Id = createdTodoList.Id,
                    Title = createdTodoList.Title,
                    Description = createdTodoList.Description,
                    CreatedAt = createdTodoList.CreatedAt,
                    UpdatedAt = createdTodoList.UpdatedAt
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
                    CreatedAt = todoList.CreatedAt,
                    UpdatedAt = todoList.UpdatedAt
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
                    Description = todoListModel.Description
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
                    CreatedAt = updatedTodoList.CreatedAt,
                    UpdatedAt = updatedTodoList.UpdatedAt
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
