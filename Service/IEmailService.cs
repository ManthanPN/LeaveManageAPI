﻿using LeaveManageAPI.Model;

namespace LeaveManageAPI.Service
{
    public interface IEmailService
    {
       Task SendEmailAsync(string email, string subject, string message);
    }
}
