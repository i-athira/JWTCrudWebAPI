using EmployeeAdminWebAPI.Controllers;
using JWTCrudWebAPI.Interfaces;
using JWTCrudWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MyProject.Tests.Controllers
{
    public class EmployeesControllerTests
    {
        private readonly Mock<IEmployeeRepository> mockRepository;
        private readonly Mock<ILogger<EmployeesController>> mockLogger;
        private readonly EmployeesController controller;

        public EmployeesControllerTests()
        {
            mockRepository = new Mock<IEmployeeRepository>();
            mockLogger = new Mock<ILogger<EmployeesController>>();
            controller = new EmployeesController(mockRepository.Object, mockLogger.Object);
        }

        [Fact]
        public async Task GetAllEmployees_ShouldReturnOkWithEmployees()
        {
            // Arrange
            var employees = new List<Employee>
    {
        new Employee { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com" },
        new Employee { Id = Guid.NewGuid(), Name = "Jane Smith", Email = "jane@example.com" }
    };
            mockRepository.Setup(repo => repo.GetAllEmployees()).ReturnsAsync(employees);

            // Act
            var result = await controller.GetAllEmployees();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GetAllEmployees method called.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Fetched 2 employees successfully.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Employee>>(okResult.Value);

            Assert.Equal(2, returnValue.Count());
        }


        [Fact]
        public async Task GetEmployeesById_ShouldReturnOk_WhenEmployeeExists()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = new Employee { Id = employeeId, Name = "John Doe", Email = "john@example.com" };
            mockRepository.Setup(repo => repo.GetEmployeesById(employeeId)).ReturnsAsync(employee);

            // Act
            var result = await controller.GetEmployeesById(employeeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Employee>(okResult.Value);
            Assert.Equal(employeeId, returnValue.Id);
        }

        [Fact]
        public async Task AddEmployee_ShouldReturnOkWithAddedEmployee()
        {
            // Arrange
            var addEmployeeDto = new AddEmployeeDto
            {
                Name = "John Doe",
                Email = "john@example.com",
                Phone = "1234567890",
                Salary = 50000
            };

            var addedEmployee = new Employee
            {
                Id = Guid.NewGuid(),
                Name = addEmployeeDto.Name,
                Email = addEmployeeDto.Email,
                Phone = addEmployeeDto.Phone,
                Salary = addEmployeeDto.Salary
            };

            mockRepository.Setup(repo => repo.AddEmployee(It.IsAny<Employee>()))
                          .Callback<Employee>(e => e.Id = addedEmployee.Id);
            mockRepository.Setup(repo => repo.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            var result = await controller.AddEmployee(addEmployeeDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Employee>(okResult.Value);
            Assert.Equal(addEmployeeDto.Name, returnValue.Name);
        }

        [Fact]
        public async Task UpdateEmployee_ShouldReturnOk_WhenEmployeeExists()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var existingEmployee = new Employee
            {
                Id = employeeId,
                Name = "John Doe",
                Email = "john@example.com",
                Phone = "1234567890",
                Salary = 50000
            };

            var updateEmployeeDto = new UpdateEmployeeDto
            {
                Name = "John Smith",
                Email = "johnsmith@example.com",
                Phone = "9876543210",
                Salary = 60000
            };

            mockRepository.Setup(repo => repo.GetEmployeesById(employeeId)).ReturnsAsync(existingEmployee);
            mockRepository.Setup(repo => repo.UpdateEmployee(existingEmployee));
            mockRepository.Setup(repo => repo.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            var result = await controller.UpdateEmployee(employeeId, updateEmployeeDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedEmployee = Assert.IsType<Employee>(okResult.Value);
            Assert.Equal(updateEmployeeDto.Name, updatedEmployee.Name);
        }

        [Fact]
        public async Task DeleteEmployee_ShouldReturnOk_WhenEmployeeExists()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var existingEmployee = new Employee { Id = employeeId, Name = "John Doe", Email = "john@gmail.com" };
            mockRepository.Setup(repo => repo.GetEmployeesById(employeeId)).ReturnsAsync(existingEmployee);
            mockRepository.Setup(repo => repo.DeleteEmployee(existingEmployee));
            mockRepository.Setup(repo => repo.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            var result = await controller.DeleteEmployee(employeeId);

            // Assert
            Assert.IsType<OkResult>(result);
        }
    }
}
