using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            this._userService = userService;
            this._logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> SearchByEmail(string query)
        {
            try
            {
                var results = await this._userService.SearchUsersByEmailAsync(query ?? string.Empty);
                return this.Json(results);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error searching users by email");
                return this.StatusCode(500, "An error occurred while searching for users");
                throw;
            }
        }
    }
}
