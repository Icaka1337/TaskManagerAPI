namespace TaskManagerAPI.DTOs
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int ProjectId { get; set; }
        public ProjectDto Project { get; set; }
        public List<UserTaskDto> UserTasks { get; set; }
    }
}
