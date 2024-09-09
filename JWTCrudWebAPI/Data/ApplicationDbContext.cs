using JWTCrudWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace JWTCrudWebAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
    }
}
