namespace HRSystem.Core.Interfaces;
using HRSystem.Core.Models;

// ─── Generic Repository ──────────────────────────────
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

// ─── Employee Repository ─────────────────────────────
public interface IEmployeeRepository : IRepository<Employee>
{
    Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<Employee>> GetByManagerAsync(int managerId);
    Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber);
    Task<string> GenerateEmployeeNumberAsync();
}

// ─── Department Repository ───────────────────────────
public interface IDepartmentRepository : IRepository<Department>
{
    Task<bool> HasEmployeesAsync(int departmentId);
}

// ─── Leave Repository ────────────────────────────────
public interface ILeaveRepository : IRepository<LeaveRequest>
{
    Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(int employeeId);
    Task<IEnumerable<LeaveRequest>> GetPendingRequestsAsync();
    Task<int> GetApprovedDaysAsync(int employeeId, LeaveType type, int year);
}

// ─── Attendance Repository ───────────────────────────
public interface IAttendanceRepository : IRepository<AttendanceRecord>
{
    Task<AttendanceRecord?> GetTodayRecordAsync(int employeeId);
    Task<IEnumerable<AttendanceRecord>> GetByEmployeeAndDateRangeAsync(
        int employeeId, DateTime startDate, DateTime endDate);
}

// ─── User Repository ─────────────────────────────────
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
}