using LeaveManageAPI.Model;
using LeaveManageAPI.Service;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace LeaveManageAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Manager, Employee")]
    [EnableCors("AllowOrigin")]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly leaveDBContext _context;
        private readonly IEmailService _emailService;

        public UsersController(IConfiguration config, leaveDBContext context, IEmailService emailService)
        {
            _config = config;
            _context = context;
            _emailService = emailService;
        }

        
        [HttpGet]
        [AllowAnonymous]
        [Route("GetEmployees")]
        public async Task<ActionResult> GetEmployees()
        {
            try
            {
                var employees = await _context.register
                    .Select(e => new
                    {
                        e.Id,
                        e.Username,
                        e.Password,
                        e.Role,
                        e.Team,
                        e.LeaveDays,
                        Email = e.Email ?? "N/A",
                        Birthdate = e.Birthdate ?? "N/A"
                    })
                    .ToListAsync();

                if (!employees.Any())
                {
                    return NotFound(new { message = "No employees found." });
                }

                return Ok(new { employees });
            }
            catch (Exception ex)
            { 
                return StatusCode(500, new { message = "An error occurred while fetching employees.", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetUser/{id}")]
        public async Task<ActionResult> GetUser(int id)
        {
            try
            {
                var employee = await _context.register
                    .Where(e => e.Id == id)
                    .Select(e => new
                    {
                        e.Id,
                        e.Username,
                        e.Password,
                        e.Role,
                        e.Team,
                        e.LeaveDays,
                        Email = e.Email ?? "N/A",
                        Birthdate = e.Birthdate ?? "N/A"
                    })
                    .FirstOrDefaultAsync();

                if (employee == null)
                {
                    return NotFound(new { message = $"No employee found with ID {id}." });
                }

                return Ok(new { employee });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the user.", error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel employee)
        {
            if (employee == null || string.IsNullOrEmpty(employee.Username) ||
                string.IsNullOrEmpty(employee.Password) || string.IsNullOrEmpty(employee.Role) || string.IsNullOrEmpty(employee.Team))
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

        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
             if (loginModel == null || string.IsNullOrEmpty(loginModel.Username) || string.IsNullOrEmpty(loginModel.Password))
             {
                 return BadRequest(new { message = "Invalid login data." });
             }

             var employee = await _context.register
                 .FirstOrDefaultAsync(e => e.Username == loginModel.Username && e.Password == loginModel.Password);

             if (employee == null)
             {
                 return Unauthorized(new { message = "Invalid username or password." });
             }

             // Generate JWT token
             var claims = new[]
             {
                 new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                 new Claim(ClaimTypes.Role, employee.Role),
                 new Claim("Id", employee.Id.ToString()),
             };

             var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
             var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

             var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials
             );

             return Ok(new
             {
                 Token = new JwtSecurityTokenHandler().WriteToken(token),
                 Employee = new
                 {
                     employee.Id,
                     employee.Username,
                     employee.Role,
                     employee.Team
                 }
             });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return Ok("Logged out successfully.");
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("roles")]
        public IActionResult GetRoles()
        {
            var roles = new[] { "Employee", "Team Leader", "Manager"};
            return Ok(roles);
        } 

        [AllowAnonymous]
        [HttpGet]
        [Route("GetTeams")]
        public IActionResult GetTeams()
        {
            var teams = new[] { "Angular", "Dotnet" };
            return Ok(teams);
        }

        [AllowAnonymous]
        [HttpPut]
        [Route("UpdateUser")]
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

        [HttpPost]
        [Route("SendOTP")]
        public async Task<IActionResult> SendOTP([FromBody] OTPRequest request, [FromServices] IEmailService emailService)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Email is required.");
            }

            var users = await _context.register.Where(u => u.Email == request.Email).ToListAsync();

            if (users.Count == 0)
            {
                return NotFound(new { message = "No user found with the provided email." });
            }

            if (users.Count > 1)
            {
                return BadRequest(new { message = "Duplicate email found. Please contact support." });
            }

            var user = users.First();
            var otp = new Random().Next(100000, 999999);

            Console.WriteLine($"Generated OTP for {user.Email}: {otp}");

            try
            {
                await emailService.SendEmailAsync(user.Email, "Your OTP Code", $"Your OTP is {otp}");
                return Ok(new { message = "OTP sent successfully to your registered email." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send OTP. Please try again later.", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword req)
        {
            if (req.Id <= 0 || string.IsNullOrEmpty(req.OTP) || string.IsNullOrEmpty(req.NewPassword))
            {
                return BadRequest(new { message = "UserId, OTP, and NewPassword are required." });
            }

            var user = await _context.register.FirstOrDefaultAsync(u => u.Id == req.Id && u.Password == req.OTP);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid OTP or User ID." });
            }

            user.Password = req.NewPassword;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successfully." , user.Password});
        }


    }
}