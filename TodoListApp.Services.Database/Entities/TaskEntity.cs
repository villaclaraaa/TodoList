using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApp.Services.Database.Entities;
public class TaskEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }
    public string Description { get; set; }

    [Required]
    public int TodoListId { get; set; }

    [ForeignKey("TodoListId")]
    public TodoListEntity TodoList { get; set; }
    public TodoListApp.WebApi.Models.TaskStatus Status { get; set; } = TodoListApp.WebApi.Models.TaskStatus.NotStarted;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int AssignedUserId { get; set; }
    public string TagsJson { get; set; }
    public string CommentsJson { get; set; }

}
