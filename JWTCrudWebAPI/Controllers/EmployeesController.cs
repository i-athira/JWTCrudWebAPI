
using JWTCrudWebAPI.Data;
using JWTCrudWebAPI.Interfaces;
using JWTCrudWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace EmployeeAdminWebAPI.Controllers
{
    

    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository employeeRepository;

        public EmployeesController(IEmployeeRepository employeeRepository)
        {
            this.employeeRepository = employeeRepository;
        }
        [Authorize]
        [HttpGet("GetAllEmployees")]
       
        public IActionResult GetAllEmployees()
        {
            var allEmployees = employeeRepository.GetAllEmployees();
            return Ok(allEmployees);
        }

        [Authorize]
        [HttpGet("GetAllEmployeesname")]
        public IActionResult GetAllEmployeesname()
        {
            var allEmployees = employeeRepository.GetAllEmployeesname();
            return Ok(allEmployees);
        }




        [Authorize]
        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult GetEmployeesById(Guid id)
        {
            var employee = employeeRepository.GetEmployeesById(id);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }
        [Authorize]
        [HttpPost]
        [Route("AddEmployee")]
        public IActionResult AddEmployee(AddEmployeeDto addEmployeeDto)
        {
            var employeeEntity = new Employee()
            {
                Name = addEmployeeDto.Name,
                Email = addEmployeeDto.Email,
                Phone = addEmployeeDto.Phone,
                Salary = addEmployeeDto.Salary
            };
            employeeRepository.AddEmployee(employeeEntity);
            employeeRepository.SaveChanges();
            return Ok(employeeEntity);
        }
        [Authorize]
        [HttpPut]
        [Route("{id:guid}")]
        public IActionResult UpdateEmployee(Guid id, UpdateEmployeeDto updateEmployeeDto)
        {
            var employee = employeeRepository.GetEmployeesById(id);
            if (employee == null)
            {
                return NotFound();
            }
            employee.Name = updateEmployeeDto.Name;
            employee.Email = updateEmployeeDto.Email;
            employee.Phone = updateEmployeeDto.Phone;
            employee.Salary = updateEmployeeDto.Salary;
            employeeRepository.UpdateEmployee(employee);
            employeeRepository.SaveChanges();
            return Ok(employee);
        }

      
        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult DeleteEmployee(Guid id)
        {
            var employee = employeeRepository.GetEmployeesById(id);
            if (employee == null)
            {
                return NotFound();
            }
            employeeRepository.DeleteEmployee(employee);
            employeeRepository.SaveChanges();
            return Ok();
        }
    }
}
