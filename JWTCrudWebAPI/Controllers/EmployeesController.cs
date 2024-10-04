
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
        [Route("Employee/{id:guid}")]
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


        [HttpPost]
        [Route("AddImage")]
        public IActionResult AddImage(ImageDTO imageDTO)
        {


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate the base64 string format
            if (string.IsNullOrEmpty(imageDTO.Base64Image))
            {
                return BadRequest("Base64 image string cannot be empty.");
            }

          

            try
            {
                // Convert base64 string to byte[]
                byte[] imageData = Convert.FromBase64String(imageDTO.Base64Image);

                // Create a new Image entity
                var imageentity = new Image
                {
                   
                    Id = imageDTO.Id,
                    Base64Image = imageData  // Assign the converted byte array here
                };

                employeeRepository.AddImage(imageentity); // Add the image to the repository
                employeeRepository.SaveChanges();// Save changes and retrieve ImageId
                // Check if ImageId is being populated correctly
                if (imageentity.ImageId == 0)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve the ImageId.");
                }


                // Return the ImageId in the response
                var response = new
                {
                    imageid = imageentity.ImageId,  // Get the generated ImageId
                    Message = "Image uploaded successfully."
                };

                return Ok(response);
            }

            catch (FormatException)
            {
                return BadRequest("Invalid base64 string format.");
            }



        }

     

        [HttpGet]
        [Route("Images/{ImageId:int}")]
        public IActionResult GetImagesByImageId(int ImageId)
        {
            var images = employeeRepository.GetImagesByImageId(ImageId);
            if (images == null || !images.Any())
            {
                return NotFound("No images found for the specified employee.");
            }

            // Assuming 'images' is a collection of entities that include ImageId and the Blob data
            var imageDtos = images.Select(image => new ImageDTO
            {
               Id= image.Id,
               Base64Image = Convert.ToBase64String(image.Base64Image) // Assuming Base64Image is a byte[]
            }).ToList();

            return Ok(imageDtos);
        }
    }
}
