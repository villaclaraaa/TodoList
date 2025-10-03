using Microsoft.AspNetCore.Identity;
using TodoListApp.Identity.Models;

namespace TodoListApp.WebApp.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            this._userManager = userManager;
        }

        public async Task<string> GetUserFullNameAsync(Guid userId)
        {
            var user = await this.GetUserByIdAsync(userId);
            if (user == null)
            {
                return "Unknown User";
            }

            return $"{user.FirstName} {user.LastName}";
        }

        public async Task<ApplicationUser> GetUserByIdAsync(Guid userId)
        {
            return await this._userManager.FindByIdAsync(userId.ToString());
        }

        public async Task<List<UserSearchResult>> SearchUsersByEmailAsync(string emailQuery)
        {
            if (string.IsNullOrWhiteSpace(emailQuery))
            {
                return new List<UserSearchResult>();
            }

            var users = this._userManager.Users
                .Where(u => u.Email.Contains(emailQuery))
                .Take(10)
                .ToList();

            return users.Select(u => new UserSearchResult
            {
                Id = u.Id,
                Email = u.Email,
                FullName = $"{u.FirstName} {u.LastName}"
            }).ToList();
        }
    }
}
