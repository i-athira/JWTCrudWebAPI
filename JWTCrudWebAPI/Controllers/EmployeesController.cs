
using JWTCrudWebAPI.Data;
using JWTCrudWebAPI.Interfaces;
using JWTCrudWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace EmployeeAdminWebAPI.Controllers
{
    

    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository employeeRepository;
        private readonly ILogger<EmployeesController> logger;

        public EmployeesController(IEmployeeRepository employeeRepository, ILogger<EmployeesController> logger)
        {
            this.employeeRepository = employeeRepository;
            this.logger = logger;
        }
        [Authorize]
        [HttpGet("GetAllEmployees")]
       
        public async Task<IActionResult> GetAllEmployees()
        {
            logger.LogInformation("GetAllEmployees method called.");
            var allEmployees = await employeeRepository.GetAllEmployees();

            if (allEmployees == null || !allEmployees.Any())
            {
                logger.LogWarning("No employees found.");
                return NotFound();
            }

            logger.LogInformation($"Fetched {allEmployees.Count()} employees successfully.");
            return Ok(allEmployees);

        }


        [Authorize]
        [HttpGet("GetAllEmployeesname")]
        public async Task<IActionResult> GetAllEmployeesname()
        {
            logger.LogInformation("GetAllEmployeesname method called.");
            try
            {

                var allEmployees = await employeeRepository.GetAllEmployeesname();
                logger.LogInformation("Fetched employee names successfully.");

                return Ok(allEmployees);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while fetching employees.");
                return StatusCode(500, "Internal server error");
            }
        }




        [Authorize]
        [HttpGet]
        [Route("Employee/{id:guid}")]
        public async Task<IActionResult> GetEmployeesById(Guid id)
        {
            logger.LogInformation("GetEmployeesById method called with ID: {Id}", id);
            try
            {
                var employee =await employeeRepository.GetEmployeesById(id);
                if (employee == null)
                {
                    logger.LogWarning("Employee with ID: {Id} not found.", id);

                    return NotFound();
                }
                logger.LogInformation("Employee with ID: {Id} fetched successfully.", id);

                return Ok(employee);

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while fetching employee with ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        [Authorize]
        [HttpPost]
        [Route("AddEmployee")]
        public async Task<IActionResult> AddEmployee(AddEmployeeDto addEmployeeDto)
        {
            logger.LogInformation("AddEmployee method called.");
            try
            {
                var employeeEntity = new Employee()
                {
                    Name = addEmployeeDto.Name,
                    Email = addEmployeeDto.Email,
                    Phone = addEmployeeDto.Phone,
                    Salary = addEmployeeDto.Salary
                };
                await employeeRepository.AddEmployee(employeeEntity);
                await employeeRepository.SaveChanges();
                logger.LogInformation("Employee added successfully with ID: {Id}", employeeEntity.Id);
                return Ok(employeeEntity);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while adding employee.");
                return StatusCode(500, "Internal server error");
            }
        }
        [Authorize]
        [HttpPut]
        [Route("{id:guid}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, UpdateEmployeeDto updateEmployeeDto)
        {
            logger.LogInformation("UpdateEmployee method called for ID: {Id}", id);

            try
            {
                var employee = await employeeRepository.GetEmployeesById(id);
                if (employee == null)
                {
                    logger.LogWarning("Employee with ID: {Id} not found for update.", id);
                    return NotFound();
                }

                // Only update Name if it is provided (null check)
              

                // Only update Email if it is provided (null check)
                if (!string.IsNullOrEmpty(updateEmployeeDto.Email))
                {
                    employee.Email = updateEmployeeDto.Email;
                }

                // Only update Phone if it is provided (null check)
                if (!string.IsNullOrEmpty(updateEmployeeDto.Phone))
                {
                    employee.Phone = updateEmployeeDto.Phone;
                }

                // Only update Salary if it is provided (null check) and not zero (optional validation)
                if (updateEmployeeDto.Salary.HasValue)
                {
                    employee.Salary = updateEmployeeDto.Salary.Value;
                }

                // Save updated employee
                await employeeRepository.UpdateEmployee(employee);
                await employeeRepository.SaveChanges();

                logger.LogInformation("Employee with ID: {Id} updated successfully.", id);
                return Ok(employee);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while updating employee with ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            logger.LogInformation("DeleteEmployee method called for ID: {Id}", id);
            try
            {
                var employee = await employeeRepository.GetEmployeesById(id);
                if (employee == null)
                {
                    logger.LogWarning("Employee with ID: {Id} not found for deletion.", id);

                    return NotFound();
                }
                await employeeRepository.DeleteEmployee(employee);
                await employeeRepository.SaveChanges();
                logger.LogInformation("Employee with ID: {Id} deleted successfully.", id);

                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while deleting employee with ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost]
        [Route("AddImage")]
        public async Task<IActionResult> AddImage(ImageDTO imageDTO)
        {
            logger.LogInformation("AddImage method called.");
            try
            {

                if (!ModelState.IsValid)
            {
                    logger.LogWarning("Invalid model state for AddImage.");

                    return BadRequest(ModelState);
            }

            // Validate the base64 string format
            if (string.IsNullOrEmpty(imageDTO.Base64Image))
                {
                    logger.LogWarning("Base64 image string is empty.");

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

                    await employeeRepository.AddImage(imageentity); // Add the image to the repository
                    await employeeRepository.SaveChanges();// Save changes and retrieve ImageId
                                                           // Check if ImageId is being populated correctly
                    if (imageentity.ImageId == 0)
                    {
                        logger.LogError("Failed to retrieve the ImageId after saving.");

                        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve the ImageId.");
                    }

                    logger.LogInformation("Image uploaded successfully with ImageId: {ImageId}", imageentity.ImageId);

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
                    logger.LogWarning("Invalid base64 string format.");

                    return BadRequest("Invalid base64 string format.");
                }
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while uploading the image.");
                return StatusCode(500, "Internal server error");
            }



        }

     

        [HttpGet]
        [Route("Images/{imageId:int}")]
        public async Task<IActionResult> GetImagesByImageId(int imageId)
        {
            logger.LogInformation("GetImagesByImageId method called with ImageId: {ImageId}", imageId);
            try
            {
                var images = await employeeRepository.GetImagesByImageId(imageId);
                if (images == null || !images.Any())
                {
                    logger.LogWarning("No images found for ImageId: {ImageId}", imageId);

                    return NotFound("No images found for the specified employee.");
                }

                // Assuming 'images' is a collection of entities that include ImageId and the Blob data
                var imageDtos = images.Select(image => new ImageDTO
                {
                    Id = image.Id,
                    Base64Image = Convert.ToBase64String(image.Base64Image), // Assuming Base64Image is a byte[]
                    ImageId = image.ImageId
                });
                logger.LogInformation("{Count} images retrieved for ImageId: {ImageId}", images.Count(), imageId);

                return Ok(imageDtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while fetching images for ImageId: {ImageId}", imageId);
                return StatusCode(500, "Internal server error");
            }
        }





        [HttpGet]
        [Route("Employee/{Id:guid}/Images")]
        public async Task<IActionResult> GetImagesByEmployeeId(Guid Id)
        {
            logger.LogInformation("GetImagesByEmployeeId method called with EmployeeId: {EmployeeId}", Id);
            try
            {
                var images = await employeeRepository.GetImagesByEmployeeId(Id);
                if (images == null || !images.Any())
                {
                    logger.LogWarning("No images found for EmployeeId: {EmployeeId}", Id);
                    return NotFound("No images found for the specified employee.");
                }

                var imageDtos = images.Select(image => new ImageDTO
                {
                    ImageId = image.ImageId,
                    Base64Image = Convert.ToBase64String(image.Base64Image), // Assuming Base64Image is a byte[]
                    Id = image.Id
                }).ToList();
                logger.LogInformation("{Count} images retrieved for EmployeeId: {EmployeeId}", imageDtos.Count, Id);

                return Ok(imageDtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while fetching images for EmployeeId: {EmployeeId}", Id);
                return StatusCode(500, "Internal server error");
            }
        }



    }
}
