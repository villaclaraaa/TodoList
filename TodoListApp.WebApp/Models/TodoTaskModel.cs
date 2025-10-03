using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models;

public class TodoTaskModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Task title is required")]
    public string Title { get; set; }
    public string Description { get; set; }
    public int TodoListId { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AssignedUserId { get; set; }
    public bool IsOverdue => this.DueDate.HasValue && this.DueDate.Value < DateTime.UtcNow && this.Status != TaskStatus.Completed;
    public bool IsCompleted => this.Status == TaskStatus.Completed;

    public ICollection<string> Tags { get; set; } = new List<string>();
    public ICollection<string> Comments { get; set; } = new List<string>();
}
