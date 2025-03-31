using LeaveManageAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManageAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly IConfiguration _config;
        public readonly leaveDBContext _context;
        public LeaveController(IConfiguration config, leaveDBContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpGet("GetLeaveTypes")]
        public IActionResult GetLeaveTypes()
        {
            var types = new[] { "Annual Leave", "Sick Leave", "Paid Leave", "Casual Leave" };
            return Ok(types);
        }

        [HttpGet("GetLeaveDurations")]
        public IActionResult GetLeaveDurations()
        {
            var durations = new[] { "AM", "PM", "Full Day" };
            return Ok(durations);
        }

        // GET: api/leave/getleaveapplications
        [HttpGet("GetLeaveApplications")]
        public async Task<IActionResult> GetLeaveApplications()
        {
            var leaveApplications = await _context.leave.ToListAsync();
            return Ok(leaveApplications);
        }

        [HttpPost("AddLeave")]
        public async Task<IActionResult> AddLeave([FromBody] LeaveApplication leave)
        {
            if (_context.leave.Any(a => a.Id == leave.Id))
            {
                return Conflict("Leave application already exists.");
            }

            if (leave == null ||
                string.IsNullOrEmpty(leave.Username) ||
                string.IsNullOrEmpty(leave.StartDate) ||
                string.IsNullOrEmpty(leave.EndDate) ||
                string.IsNullOrEmpty(leave.Reason) ||
                string.IsNullOrEmpty(leave.Team) ||
                string.IsNullOrEmpty(leave.TypeLeave) ||
                string.IsNullOrEmpty(leave.LeaveDuration)
                )
            {
                return BadRequest("Invalid employee data.");
            }

            
            leave.Status = "pending";

            _context.leave.Add(leave);
            await _context.SaveChangesAsync();
            return Ok(leave);
        }

        // PATCH: api/leave/approveleave/{id}
        [HttpPatch("ApproveLeave/{id}")]
        public async Task<IActionResult>  ApproveLeave(int id)
        {
            var leaveApplication = await _context.leave.FindAsync(id);
            if (leaveApplication == null)
            {
                return NotFound("Leave application not found.");
            }

            leaveApplication.Status = "approved";
            await _context.SaveChangesAsync();
            return Ok(leaveApplication);
        }

        // PATCH: api/leave/rejectleave/{id}
        [HttpPatch("RejectLeave/{id}")]
        public async Task<IActionResult> RejectLeave(int id)
        {
            var leaveApplication = await _context.leave.FindAsync(id);
            if (leaveApplication == null)
            {
                return NotFound("Leave application not found.");
            }

            leaveApplication.Status = "rejected";
            await _context.SaveChangesAsync();
            return Ok(leaveApplication);
        }

        // DELETE: api/leave/deleteleave/{id}
        [HttpDelete("DeleteLeave/{id}")]
        public async Task<IActionResult> DeleteLeave(int id)
        {
            var leaveApplication = await _context.leave.FindAsync(id);
            if (leaveApplication == null)
            {
                return NotFound("Leave application not found.");
            }

            _context.leave.Remove(leaveApplication);
            await _context.SaveChangesAsync();
            return Ok(leaveApplication);
        }
    }
}

