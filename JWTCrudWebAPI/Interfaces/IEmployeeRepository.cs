using JWTCrudWebAPI.Models;

namespace JWTCrudWebAPI.Interfaces
{
    public interface IEmployeeRepository
    {
        IEnumerable<Employee> GetAllEmployees();
        Employee GetEmployeesById(Guid id);
        IEnumerable<Employee> GetAllEmployeesname();

        void AddEmployee(Employee employee);
        void UpdateEmployee(Employee employee);
        void DeleteEmployee(Employee employee);
        void SaveChanges();
    }
}
