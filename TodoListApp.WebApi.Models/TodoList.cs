namespace TodoListApp.WebApi.Models
{
    public class TodoList
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid OwnerId { get; set; }
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}
