using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskManagerAPI.Models;


namespace TaskManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.Include(u => u.UserTasks).ThenInclude(ut => ut.Task).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return user;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<User>>> SearchUsers(string username)
        {
            return await _context.Users
                .Where(u => u.Username.Contains(username))
                .ToListAsync();
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == login.Username && u.PasswordHash == ComputeHash(login.Password));
            if (user == null)
            {
                return Unauthorized();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            // Check if the username already exists
            if (_context.Users.Any(u => u.Username == model.Username))
            {
                return Conflict("Username already exists");
            }

            // Check if the email already exists
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                return Conflict("Email already exists");
            }

            // Hash the password before storing it in the database
            string hashedPassword = ComputeHash(model.Password);

            // Create a new user entity
            var user = new User
            {
                Username = model.Username,
                PasswordHash = hashedPassword,
                Email = model.Email,
            };

            // Add the user to the database
            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User registered successfully");
        }

        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        /*[HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }*/

         [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Retrieve the user ID from the claims in the JWT token
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(); // User ID claim not found in the token
            }

            var userId = int.Parse(userIdClaim.Value);

            // Ensure that the authenticated user matches the user being deleted
            if (userId != id)
            {
                return Forbid(); // User is not authorized to delete other users
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(); // User not found
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent(); // Successful deletion
        }

        [HttpPost("{userId}/tasks/{taskId}")]
        public async Task<IActionResult> AssignTaskToUser(int userId, int taskId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return NotFound();
            }

            var userTask = new UserTask
            {
                UserId = userId,
                TaskId = taskId,
                AssignedDate = DateTime.UtcNow
            };

            _context.UserTasks.Add(userTask);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private string ComputeHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
