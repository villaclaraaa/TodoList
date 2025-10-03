using TodoListApp.Identity.Models;

namespace TodoListApp.WebApp.Services
{
    public interface IUserService
    {
        Task<string> GetUserFullNameAsync(Guid userId);
        Task<ApplicationUser> GetUserByIdAsync(Guid userId);
        Task<List<UserSearchResult>> SearchUsersByEmailAsync(string emailQuery);

    }
    public class UserSearchResult
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }
}
