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
        public DbSet<Image> Images { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<Image>()
                .HasKey(i => i.ImageId);

            modelBuilder.Entity<Image>()
                .HasOne(i => i.Employee)
                .WithMany() // Assuming an employee can have multiple images
                .HasForeignKey(i => i.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
