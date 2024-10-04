using JWTCrudWebAPI.Models;

namespace JWTCrudWebAPI.Interfaces
{
    public interface IEmployeeRepository
    {
        IEnumerable<Employee> GetAllEmployees();
        Employee GetEmployeesById(Guid id);
        IEnumerable<Employee> GetAllEmployeesname();

        IEnumerable<Image> GetImagesByEmployeeId(Guid id);
        IEnumerable<Image> GetImagesByImageId(int ImageId);
        void AddImage(Image image);

        void AddEmployee(Employee employee);
        void UpdateEmployee(Employee employee);
        void DeleteEmployee(Employee employee);
        void SaveChanges();
    }
}
