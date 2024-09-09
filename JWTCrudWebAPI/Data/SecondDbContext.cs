using JWTCrudWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace JWTCrudWebAPI.Data
{
    public class SecondDbContext : DbContext
    {
        public SecondDbContext(DbContextOptions<SecondDbContext> options) : base(options)
        {

        }
        public DbSet<Employee> Employees { get; set; }
    }
}
