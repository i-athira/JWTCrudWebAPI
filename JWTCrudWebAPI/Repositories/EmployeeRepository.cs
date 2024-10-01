using JWTCrudWebAPI.Data;
using JWTCrudWebAPI.Interfaces;
using JWTCrudWebAPI.Models;
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
        public void AddEmployee(Employee employee)
        {
            dbContext.Employees.Add(employee);
        }

        public void DeleteEmployee(Employee employee)
        {
            dbContext.Employees.Remove(employee);
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return dbContext.Employees.ToList();
        }

        public IEnumerable<Employee> GetAllEmployeesname()
        {
            return dbContext.Employees
                      .Where(e => e.Name.StartsWith("A") )
                      .OrderBy(e => e.Name)
                      .ToList();
        }

        public Employee GetEmployeesById(Guid id)
        {
            return dbContext.Employees.Find(id);
        }

        public void SaveChanges()
        {
            dbContext.SaveChanges();
        }

        public void UpdateEmployee(Employee employee)
        {
            dbContext.Employees.Update(employee);
        }
    }
}
