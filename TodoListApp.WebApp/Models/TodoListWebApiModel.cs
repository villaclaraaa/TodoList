using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models;

public class TodoListWebApiModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; }

    public string Description { get; set; }

    [Required]
    public int OwnerId { get; set; }
    public List<TodoTaskWebApiModel> Tasks { get; set; } = new List<TodoTaskWebApiModel>();
}
