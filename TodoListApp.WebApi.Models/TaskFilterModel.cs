namespace TodoListApp.WebApi.Models
{
    public class TaskFilterModel
    {
        public int? TodoListId { get; set; }
        public TaskStatus? Status { get; set; }
        public int? AssignedUserId { get; set; }
        public string TagFilter { get; set; }

        // Due date range
        public DateTime? DueDateStart { get; set; }
        public DateTime? DueDateEnd { get; set; }

        // Creation time range
        public DateTime? CreatedAtStart { get; set; }
        public DateTime? CreatedAtEnd { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
