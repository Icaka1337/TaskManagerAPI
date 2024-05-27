namespace TaskManagerAPI.DTOs
{
    public class UserDetailsDto : UserDto
    {
        public List<TaskDto> Tasks { get; set; }
    }
}
