using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace LeaveManageAPI.Model
{
    public class leaveDBContext : DbContext
    {
    
        public leaveDBContext(DbContextOptions options) : base(options) {}
        public DbSet<LeaveApplication> leave { get; set; }
        public DbSet<RegisterModel> register { get; set; }
        /*public DbSet<UpdateUserModel> update { get; set; }*/
    }
}
