using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Services.Database.Entities;
public class TodoListEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; }
    public string Description { get; set; }

    [Required]
    public Guid OwnerId { get; set; }
    public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
}
