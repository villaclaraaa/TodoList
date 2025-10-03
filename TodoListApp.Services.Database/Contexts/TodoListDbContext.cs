using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;

namespace TodoListApp.Services.Database.Contexts
{
    public class TodoListDbContext : DbContext
    {
        public TodoListDbContext(DbContextOptions<TodoListDbContext> options) : base(options)
        {
        }

        public DbSet<TodoListEntity> TodoLists { get; set; }
        public DbSet<TaskEntity> Tasks { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TodoListEntity
            modelBuilder.Entity<TodoListEntity>().ToTable("TodoLists");

            // Configure TaskEntity
            modelBuilder.Entity<TaskEntity>().ToTable("Tasks");

            // Configure relationship
            modelBuilder.Entity<TaskEntity>()
                .HasOne(t => t.TodoList)
                .WithMany(tl => tl.Tasks)  // Specify the Tasks collection in TodoListEntity
                .HasForeignKey(t => t.TodoListId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskEntity>()
                .Property(t => t.TagsJson)
                .IsRequired(false);

            modelBuilder.Entity<TaskEntity>()
                .Property(t => t.CommentsJson)
                .IsRequired(false);
        }
    }
}
