using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Models;
using TaskManagerAPI.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = TaskManagerAPI.Models.Task;

namespace TaskManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            var projects = await _context.Projects
                .Include(p => p.Tasks)
                .ToListAsync();

            var projectDtos = projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Budget = p.Budget,
                Tasks = p.Tasks.Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    ProjectId = t.ProjectId
                }).ToList()
            }).ToList();

            return projectDtos;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var projectDto = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Budget = project.Budget,
                Tasks = project.Tasks.Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    ProjectId = t.ProjectId
                }).ToList()
            };

            return projectDto;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> SearchProjects(string name)
        {
            var projects = await _context.Projects
                .Where(p => p.Name.Contains(name))
                .Include(p => p.Tasks)
                .ToListAsync();

            var projectDtos = projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Budget = p.Budget,
                Tasks = p.Tasks.Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    ProjectId = t.ProjectId
                }).ToList()
            }).ToList();

            return projectDtos;
        }

        [HttpPost("PostProject")]
        public async Task<ActionResult<ProjectDto>> PostProject(ProjectDto projectDto)
        {
            var project = new Project
            {
                Name = projectDto.Name,
                Description = projectDto.Description,
                StartDate = projectDto.StartDate,
                EndDate = projectDto.EndDate,
                Budget = projectDto.Budget,
                Tasks = projectDto.Tasks.Select(t => new Task
                {
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    ProjectId = t.ProjectId
                }).ToList()
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            projectDto.Id = project.Id;

            return CreatedAtAction(nameof(GetProject), new { id = projectDto.Id }, projectDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, ProjectDto projectDto)
        {
            if (id != projectDto.Id)
            {
                return BadRequest();
            }

            var project = await _context.Projects.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            project.Name = projectDto.Name;
            project.Description = projectDto.Description;
            project.StartDate = projectDto.StartDate;
            project.EndDate = projectDto.EndDate;
            project.Budget = projectDto.Budget;

            // Update tasks
            project.Tasks.Clear();
            project.Tasks = projectDto.Tasks.Select(t => new Task
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                ProjectId = t.ProjectId
            }).ToList();

            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
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
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
