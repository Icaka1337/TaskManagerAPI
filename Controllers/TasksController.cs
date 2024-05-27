using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TaskManagerAPI.Models;
using TaskManagerAPI.DTOs;
using Task = TaskManagerAPI.Models.Task;

namespace TaskManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks()
        {
            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.UserTasks)
                    .ThenInclude(ut => ut.User)
                .ToListAsync();

            return tasks.Select(task => new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                ProjectId = task.ProjectId,
                Project = new ProjectDto
                {
                    Id = task.Project.Id,
                    Name = task.Project.Name,
                    Description = task.Project.Description,
                    StartDate = task.Project.StartDate,
                    EndDate = task.Project.EndDate,
                    Budget = task.Project.Budget
                },
                UserTasks = task.UserTasks.Select(ut => new UserTaskDto
                {
                    UserId = ut.UserId,
                    TaskId = ut.TaskId,
                    AssignedDate = ut.AssignedDate,
                    User = new UserDto
                    {
                        Id = ut.User.Id,
                        Username = ut.User.Username,
                        Email = ut.User.Email
                    }
                }).ToList()
            }).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.UserTasks)
                    .ThenInclude(ut => ut.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            var taskDto = new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                ProjectId = task.ProjectId,
                Project = new ProjectDto
                {
                    Id = task.Project.Id,
                    Name = task.Project.Name,
                    Description = task.Project.Description,
                    StartDate = task.Project.StartDate,
                    EndDate = task.Project.EndDate,
                    Budget = task.Project.Budget
                },
                UserTasks = task.UserTasks.Select(ut => new UserTaskDto
                {
                    UserId = ut.UserId,
                    TaskId = ut.TaskId,
                    AssignedDate = ut.AssignedDate,
                    User = new UserDto
                    {
                        Id = ut.User.Id,
                        Username = ut.User.Username,
                        Email = ut.User.Email
                    }
                }).ToList()
            };

            return taskDto;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> SearchTasks(string title)
        {
            var tasks = await _context.Tasks
                .Where(t => t.Title.Contains(title))
                .Include(t => t.Project)
                .Include(t => t.UserTasks)
                    .ThenInclude(ut => ut.User)
                .ToListAsync();

            return tasks.Select(task => new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                ProjectId = task.ProjectId,
                Project = new ProjectDto
                {
                    Id = task.Project.Id,
                    Name = task.Project.Name,
                    Description = task.Project.Description,
                    StartDate = task.Project.StartDate,
                    EndDate = task.Project.EndDate,
                    Budget = task.Project.Budget
                },
                UserTasks = task.UserTasks.Select(ut => new UserTaskDto
                {
                    UserId = ut.UserId,
                    TaskId = ut.TaskId,
                    AssignedDate = ut.AssignedDate,
                    User = new UserDto
                    {
                        Id = ut.User.Id,
                        Username = ut.User.Username,
                        Email = ut.User.Email
                    }
                }).ToList()
            }).ToList();
        }

        [HttpPost("PostTask")]
        public async Task<ActionResult<Task>> PostTask(Task task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTask(int id, Task task)
        {
            if (id != task.Id)
            {
                return BadRequest();
            }

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
