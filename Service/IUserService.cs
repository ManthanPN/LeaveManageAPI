using LeaveManageAPI.Model;
using LeaveManageAPI.Helper;

namespace LeaveManageAPI.Service
{
    public interface IUserService
    {
        public Task<APIResponse> SendOTP(string Email, string Subject, string Body);
    }
}
