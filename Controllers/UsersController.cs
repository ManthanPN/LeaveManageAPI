using LeaveManageAPI.Model;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Runtime.InteropServices;

namespace LeaveManageAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly leaveDBContext _context;

        public UsersController(IConfiguration config, leaveDBContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpGet("GetEmployees")]
        public async Task<ActionResult<IEnumerable<RegsiterModel>>> GetEmployees()
        {
            return await _context.register.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RegsiterModel>> GetEmployee(int id)
        {
            var employee = await _context.register.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegsiterModel employee)
        {
            if (employee == null || string.IsNullOrEmpty(employee.Username) ||
                string.IsNullOrEmpty(employee.Password) || string.IsNullOrEmpty(employee.Role))
            {
                return BadRequest("Invalid employee data.");
            }

            var existingUser = await _context.register
                .FirstOrDefaultAsync(e => e.Username == employee.Username);

            if (existingUser != null)
            {
                return Conflict("User already exists.");
            }

            if (employee.Role.ToLower() == "employee" && employee.LeaveDays == 0)
            {
                employee.LeaveDays = 26;
            }

            // Adding email and birthdate during registration
            if (!string.IsNullOrEmpty(employee.Email))
            {
                employee.Email = employee.Email;
            }

            if (!string.IsNullOrEmpty(employee.Birthdate))
            {
                employee.Birthdate = employee.Birthdate;
            }

            _context.register.Add(employee);
            await _context.SaveChangesAsync();

            return Ok(employee);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            if (loginModel == null || string.IsNullOrEmpty(loginModel.Username) ||
                string.IsNullOrEmpty(loginModel.Password))
            {
                return BadRequest("Invalid login data.");
            }

            var employee = await _context.register
                .FirstOrDefaultAsync(e => e.Username == loginModel.Username && e.Password == loginModel.Password);

            if (employee == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(new { Id = employee.Id, Username = employee.Username, Role = employee.Role });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return Ok("Logged out successfully.");
        }

        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var roles = new[] { "Employee", "Manager" };
            return Ok(roles);
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserModel updateUserModel)
        {
            if (updateUserModel == null || string.IsNullOrEmpty(updateUserModel.Username))
            {
                return BadRequest("Invalid data.");
            }

            var user = await _context.register.FirstOrDefaultAsync(u => u.Id == updateUserModel.Id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Update fields
            user.Username = updateUserModel.Username;
            user.Password = updateUserModel.Password;

            if (!string.IsNullOrEmpty(updateUserModel.Email))
            {
                user.Email = updateUserModel.Email;
            }

            if (!string.IsNullOrEmpty(updateUserModel.Birthdate))
            {
                user.Birthdate = updateUserModel.Birthdate;
            }

            _context.register.Update(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }


    }
}


