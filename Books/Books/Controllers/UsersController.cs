using Books.Data;
using Books.Dtos;
using Books.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Books.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }
        //Get users
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            return Ok(_context.Users);
        }
        //Get user by id
        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound(new { Message = $"User with ID {id} not found." });
            }
            return Ok(user);
        }
        //Create user
    [HttpPost]
    public ActionResult<User> CreateUser(UserDto  userDto)
{
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

    var user = new User
    {
        Name = userDto.Name,
        Email = userDto.Email,
        Password = hashedPassword,
        Role = "User" // Standardroll
    };

    _context.Users.Add(user);
    _context.SaveChanges();

    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
}
        [HttpPut("{id}")]
        public ActionResult UpdateUser(int id, UpdateUserDto updatedUserDto)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            // Uppdatera namn och e-post
            user.Name = updatedUserDto.Name;
            user.Email = updatedUserDto.Email;

            // Kontrollera om ett nytt lösenord skickas med
            if (!string.IsNullOrEmpty(updatedUserDto.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(updatedUserDto.Password);
            }

            _context.SaveChanges();
            return Ok(new { Message = $"User with ID {id} was successfully updated." });
        }
        //Delete user
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public ActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound(new { Message = $"User with ID {id} not found." });
            }
            // Förhindra att en admin tar bort sig själv
            var loggedInUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (loggedInUserId == id)
            {
                return BadRequest(new { Message = "Admins cannot delete their own accounts." });
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return Ok(new { Message = $"User with ID {id} was successfully deleted." });
        }

    }
}
