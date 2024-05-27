﻿namespace TaskManagerAPI.DTOs
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double Budget { get; set; }
        public List<TaskDto> Tasks { get; set; }
    }
}
