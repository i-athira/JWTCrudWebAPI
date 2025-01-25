using JWTCrudWebAPI.Models;
using System.Threading.Tasks;

namespace JWTCrudWebAPI.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllEmployees();
        Task<Employee> GetEmployeesById(Guid id);
        Task<IEnumerable<Employee>> GetAllEmployeesname();

        Task<IEnumerable<Image>> GetImagesByEmployeeId(Guid id);
        Task<IEnumerable<Image>> GetImagesByImageId(int ImageId);
        Task AddImage(Image image);

        Task AddEmployee(Employee employee);
        Task UpdateEmployee(Employee employee);
        Task DeleteEmployee(Employee employee);
        Task SaveChanges();
    }
}
