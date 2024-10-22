using Microsoft.EntityFrameworkCore;
using School.Models;

namespace School.DataContext
{
    public class DataContextDb : DbContext
    {
        public DataContextDb(DbContextOptions<DataContextDb>options):base(options)
        {
        }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<AdminLogin> AdminLogins { get; set; }
        public DbSet<Login> Logins { get; set; }
        public DbSet<Register> Registers { get; set; }
        public DbSet<UserForgotPassword> UserForgotPasswords { get; set; }
    }
}
