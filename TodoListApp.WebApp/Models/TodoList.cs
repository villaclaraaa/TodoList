namespace TodoListApp.WebApp.Models;

public class TodoList
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int OwnerId { get; set; }
    public List<TodoTask> Tasks { get; set; } = new List<TodoTask>();
}
