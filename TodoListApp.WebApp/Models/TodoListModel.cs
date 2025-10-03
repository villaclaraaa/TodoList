using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models;

public class TodoListModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
    public string Title { get; set; }
    public string Description { get; set; }

    [Required(ErrorMessage = "Owner ID is required")]
    public Guid OwnerId { get; set; }
    public ICollection<TodoTaskModel> Tasks { get; set; } = new List<TodoTaskModel>();
}
