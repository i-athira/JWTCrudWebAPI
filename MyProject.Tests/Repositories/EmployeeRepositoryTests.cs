using JWTCrudWebAPI.Data;
using JWTCrudWebAPI.Models;
using JWTCrudWebAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Tests.Repositories
{
    public class EmployeeRepositoryTests
    {
        private SecondDbContext CreateDbContext()
        {
            var options=new DbContextOptionsBuilder<SecondDbContext>().UseInMemoryDatabase
                (databaseName:Guid.NewGuid().ToString()).Options;    
            return new SecondDbContext(options);   
        }
        [Fact]
        public async Task AddEmployee_ShouldAddEmployee()
        {
            var dbContext = CreateDbContext();
            var repository = new EmployeeRepository(dbContext);
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "abc@gmail.com",
                Phone = "6765543411",
                Salary = 100000
            };

            await repository.AddEmployee(employee);
            await repository.SaveChanges();  // Ensure SaveChangesAsync is used for async operations

            var employees = await dbContext.Employees.ToListAsync();  // Use ToListAsync for async fetching
            Assert.Single(employees);
            Assert.Equal("John Doe", employees.First().Name);
        }

        [Fact]

        public async Task GetAllEmployees_ShouldReturnAllEmployees()
        {
            var dbContext = CreateDbContext();
            var repository = new EmployeeRepository(dbContext);

            dbContext.Employees.AddRange(
                new Employee { Id = Guid.NewGuid(), Name = "Alice", Email = "bbc@gmail.com", Phone = "6865543411", Salary = 10000 },
                new Employee { Id = Guid.NewGuid(), Name = "Bob", Email = "bob@gmail.com", Phone = "6869543411", Salary = 100000 }
            );
            await dbContext.SaveChangesAsync();  // Use async version of SaveChanges


            var employees = await repository.GetAllEmployees();

            Assert.Equal(2, employees.Count());
            Assert.Contains(employees, e => e.Name == "Alice");
            Assert.Contains(employees, e => e.Name == "Bob");
        }
        [Fact]
        public async Task GetEmployeesById_ShouldReturnEmployee_WhenEmployeeExists()
        {
            var dbContext = CreateDbContext();
            var repository = new EmployeeRepository(dbContext);

            var employeeId = Guid.NewGuid();
            dbContext.Employees.Add(new Employee { Id = employeeId, Name = "John Doe", Email = "abc@gmail.com", Phone = "6765543411", Salary = 100000 });
            await dbContext.SaveChangesAsync();

            var employee = await repository.GetEmployeesById(employeeId);

            Assert.NotNull(employee);
            Assert.Equal("John Doe", employee.Name);
        }
        [Fact]
        public async Task DeleteEmployee_ShouldRemoveEmployeeFromDatabase()
        {
            var dbContext = CreateDbContext();
            var repository = new EmployeeRepository(dbContext);

            var employee = new Employee { Id = Guid.NewGuid(), Name = "John Doe", Email = "abc@gmail.com", Phone = "6765543411", Salary = 100000 };
            dbContext.Employees.Add(employee);
            await dbContext.SaveChangesAsync();

            await repository.DeleteEmployee(employee);
            await repository.SaveChanges();
            var employees = await dbContext.Employees.ToListAsync();
            Assert.Empty(employees);
        }

    }
}
