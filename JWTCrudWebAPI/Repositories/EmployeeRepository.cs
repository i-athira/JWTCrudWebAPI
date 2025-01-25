using JWTCrudWebAPI.Data;
using JWTCrudWebAPI.Interfaces;
using JWTCrudWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace JWTCrudWebAPI.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly SecondDbContext dbContext;

        public EmployeeRepository(SecondDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task AddEmployee(Employee employee)
        {
           await dbContext.Employees.AddAsync(employee);
        }

        public async Task AddImage(Image image)
        {
            await dbContext.Images.AddAsync(image);
        }

        public async Task DeleteEmployee(Employee employee)
        {
            dbContext.Employees.Remove(employee);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployees()
        {
            return await dbContext.Employees.ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesname()
        {
            return await dbContext.Employees
                 .Where(e => e.Name.StartsWith("A"))
                 .OrderBy(e => e.Name)
                 .ToListAsync();
        }

        public async Task<Employee> GetEmployeesById(Guid id)
        {
            return await dbContext.Employees.FindAsync(id);
        }

        public async Task<IEnumerable<Image>> GetImagesByEmployeeId(Guid id)
        {
           return await dbContext.Images.Where(e => e.Id == id).ToListAsync();
        }

        public async Task<IEnumerable<Image>> GetImagesByImageId(int imageId)
        {
            return await dbContext.Images.Where(e => e.ImageId == imageId).ToListAsync();
        }

        public async Task SaveChanges()
        {
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateEmployee(Employee employee)
        {
            dbContext.Employees.Update(employee);
            await dbContext.SaveChangesAsync();
        }

       
    }
}
