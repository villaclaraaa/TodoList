using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models;

public class TodoTaskWebApiModel
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; }

    public string Description { get; set; }

    public int TodoListId { get; set; }

    public TaskStatus Status { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public int AssignedUserId { get; set; }

    public List<string> Tags { get; set; } = new List<string>();

    public List<string> Comments { get; set; } = new List<string>();
}
