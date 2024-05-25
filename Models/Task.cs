namespace TaskManagerAPI.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public ICollection<UserTask> UserTasks { get; set; }
    }
}
