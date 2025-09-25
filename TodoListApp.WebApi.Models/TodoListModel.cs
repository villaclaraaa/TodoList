using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models
{
    public class TodoListModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        public string Description { get; set; }

        [Required]
        public int OwnerId { get; set; }
        public ICollection<TaskModel> Tasks { get; set; } = new List<TaskModel>();
    }
}
