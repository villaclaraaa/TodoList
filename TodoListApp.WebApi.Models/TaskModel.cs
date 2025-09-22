using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models;
public class TaskModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    public string Description { get; set; }

    [Required]
    public int TodoListId { get; set; }

    public TaskStatus Status { get; set; }
    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsOverdue => this.DueDate.HasValue
            && this.DueDate.Value < DateTime.UtcNow
            && this.Status != TaskStatus.Completed;

    public int AssignedUserId { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public List<string> Comments { get; set; } = new List<string>();
}
public enum TaskStatus
{
    NotStarted,
    InProgress,
    Completed
}
