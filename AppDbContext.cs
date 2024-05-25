using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Models;
using Task = TaskManagerAPI.Models.Task;

namespace TaskManagerAPI
{
    public class AppDbContext : DbContext
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.StartDate).IsRequired();
            });

            modelBuilder.Entity<Task>(entity =>
            {
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
            });

            modelBuilder.Entity<UserTask>(entity =>
            {
                entity.HasKey(ut => new { ut.UserId, ut.TaskId });
                entity.HasOne(ut => ut.User)
                    .WithMany(u => u.UserTasks)
                    .HasForeignKey(ut => ut.UserId);
                entity.HasOne(ut => ut.Task)
                    .WithMany(t => t.UserTasks)
                    .HasForeignKey(ut => ut.TaskId);
                entity.Property(ut => ut.AssignedDate).IsRequired();
            });
        }
    }

}
