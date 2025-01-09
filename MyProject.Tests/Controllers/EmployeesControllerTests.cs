using EmployeeAdminWebAPI.Controllers;
using JWTCrudWebAPI.Interfaces;
using JWTCrudWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Tests.Controllers
{
    //Use Moq to mock the IEmployeeRepository dependency and inject it into the controller.


    public class EmployeesControllerTests
    {
        private readonly Mock<IEmployeeRepository> mockRepository;

        private readonly EmployeesController controller;
        public EmployeesControllerTests()
        {
            mockRepository = new Mock<IEmployeeRepository>();
            controller = new EmployeesController(mockRepository.Object);
        }
        [Fact]
        public void GetAllEmployees_ShouldReturnOkWithEmployees()
        {
            // Arrange
            var employees = new List<Employee>
        {
            new Employee { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com" },
            new Employee { Id = Guid.NewGuid(), Name = "Jane Smith", Email = "jane@example.com" }
        };
            mockRepository.Setup(repo => repo.GetAllEmployees()).Returns(employees);

            // Act
            var result = controller.GetAllEmployees();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Employee>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
        }
        [Fact]
        public void GetEmployeesById_ShouldReturnOk_WhenEmployeeExists()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = new Employee { Id = employeeId, Name = "John Doe", Email = "john@example.com" };
            mockRepository.Setup(repo => repo.GetEmployeesById(employeeId)).Returns(employee);

            // Act
            var result = controller.GetEmployeesById(employeeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Employee>(okResult.Value);
            Assert.Equal(employeeId, returnValue.Id);
        }
        [Fact]
        public void GetEmployeesById_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
        {
            // Arrange
            mockRepository.Setup(repo => repo.GetEmployeesById(It.IsAny<Guid>())).Returns((Employee)null);

            // Act
            var result = controller.GetEmployeesById(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void AddEmployee_ShouldReturnOkWithAddedEmployee()
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

            mockRepository.Setup(repo => repo.AddEmployee(It.IsAny<Employee>()));
            mockRepository.Setup(repo => repo.SaveChanges());

            // Act
            var result = controller.AddEmployee(addEmployeeDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Employee>(okResult.Value);
            Assert.Equal(addEmployeeDto.Name, returnValue.Name);
            Assert.Equal(addEmployeeDto.Email, returnValue.Email);
        }
        [Fact]
        public void UpdateEmployee_ShouldReturnOk_WhenEmployeeExists()
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

            mockRepository.Setup(repo => repo.GetEmployeesById(employeeId))
                .Returns(existingEmployee);
            mockRepository.Setup(repo => repo.UpdateEmployee(existingEmployee));
            mockRepository.Setup(repo => repo.SaveChanges());

            // Act
            var result = controller.UpdateEmployee(employeeId, updateEmployeeDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedEmployee = Assert.IsType<Employee>(okResult.Value);
            Assert.Equal(updateEmployeeDto.Name, updatedEmployee.Name);
            Assert.Equal(updateEmployeeDto.Email, updatedEmployee.Email);
            Assert.Equal(updateEmployeeDto.Phone, updatedEmployee.Phone);
            Assert.Equal(updateEmployeeDto.Salary, updatedEmployee.Salary);
        }


        [Fact]
        public void UpdateEmployee_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
        {
            // Arrange
            mockRepository.Setup(repo => repo.GetEmployeesById(It.IsAny<Guid>()))
                .Returns((Employee)null);
            var updateEmployeeDto = new UpdateEmployeeDto
            {
                Name = "Test Name",
                Email = "test@example.com",
                Phone = "1234567890",
                Salary = 50000
            };
            // Act
            var result = controller.UpdateEmployee(Guid.NewGuid(),  updateEmployeeDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteEmployee_ShouldReturnOk_WhenEmployeeExists()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var existingEmployee = new Employee { Id = employeeId, Name = "John Doe",Email="john@gmail.com" };
            mockRepository.Setup(repo => repo.GetEmployeesById(employeeId)).Returns(existingEmployee);
            mockRepository.Setup(repo => repo.DeleteEmployee(existingEmployee));
            mockRepository.Setup(repo => repo.SaveChanges());

            // Act
            var result = controller.DeleteEmployee(employeeId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void DeleteEmployee_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
        {
            // Arrange
            mockRepository.Setup(repo => repo.GetEmployeesById(It.IsAny<Guid>())).Returns((Employee)null);

            // Act
            var result = controller.DeleteEmployee(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

    }
}
