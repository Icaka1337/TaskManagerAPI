namespace TaskManagerAPI.DTOs
{
    public class UserTaskDto
    {
        public int UserId { get; set; }
        public UserDto User { get; set; }
        public int TaskId { get; set; }
        public TaskDto Task { get; set; }
        public DateTime AssignedDate { get; set; }
    }
}
