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
        public void AddEmployee_ShouldAddEmployee()
        {
            var dbContext=CreateDbContext();
            var repository=new EmployeeRepository(dbContext);
            var employee=new Employee { Id = Guid.NewGuid(), Name = "John Doe", Email = "abc@gmail.com",Phone= "6765543411",Salary=100000 };
            repository.AddEmployee(employee);
            repository.SaveChanges();
            var employees = dbContext.Employees.ToList();
            Assert.Single(employees);
            Assert.Equal("John Doe", employees.First().Name);
        }

        [Fact]

        public void GetAllEmployees_ShouldReturnAllEmployees()
        {
            var dbContext = CreateDbContext();
            var repository = new EmployeeRepository(dbContext);

            dbContext.Employees.AddRange(
                         new Employee { Id = Guid.NewGuid(), Name = "Alice", Email = "bbc@gmail.com",Phone= "6865543411",Salary=10000 },
                         new Employee { Id = Guid.NewGuid(), Name = "Bob", Email = "bob@gmail.com", Phone = "6869543411", Salary = 100000 }
            );
            dbContext.SaveChanges();

            var employees = repository.GetAllEmployees();

            Assert.Equal(2, employees.Count());
            Assert.Contains(employees, e => e.Name == "Alice");
            Assert.Contains(employees, e => e.Name == "Bob");
        }
        [Fact]
        public void GetEmployeesById_ShouldReturnEmployee_WhenEmployeeExists()
        {
            var dbContext = CreateDbContext();
            var repository = new EmployeeRepository(dbContext);

            var employeeId = Guid.NewGuid();
            dbContext.Employees.Add(new Employee { Id = employeeId, Name = "John Doe", Email = "abc@gmail.com", Phone = "6765543411", Salary = 100000 });
            dbContext.SaveChanges();

            var employee = repository.GetEmployeesById(employeeId);

            Assert.NotNull(employee);
            Assert.Equal("John Doe", employee.Name);
        }
        [Fact]
        public void DeleteEmployee_ShouldRemoveEmployeeFromDatabase()
        {
            var dbContext = CreateDbContext();
            var repository = new EmployeeRepository(dbContext);

            var employee = new Employee { Id = Guid.NewGuid(), Name = "John Doe", Email = "abc@gmail.com", Phone = "6765543411", Salary = 100000 };
            dbContext.Employees.Add(employee);
            dbContext.SaveChanges();

            repository.DeleteEmployee(employee);
            repository.SaveChanges();

            var employees = dbContext.Employees.ToList();
            Assert.Empty(employees);
        }

    }
}
