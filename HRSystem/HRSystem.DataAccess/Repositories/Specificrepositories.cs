using Microsoft.EntityFrameworkCore;
using HRSystem.Core.Interfaces;
using HRSystem.Core.Models;

namespace HRSystem.DataAccess.Repositories;

// ─── Employee Repository ──────────────────────────────
public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(HRDbContext context) : base(context) { }

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId)
        => await _set
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .Include(e => e.Manager)
            .Where(e => e.DepartmentId == departmentId)
            .ToListAsync();

    public async Task<IEnumerable<Employee>> GetByManagerAsync(int managerId)
        => await _set
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .Where(e => e.ManagerId == managerId)
            .ToListAsync();

    public async Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber)
        => await _set
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber);

    public async Task<string> GenerateEmployeeNumberAsync()
    {
        var lastEmployee = await _set
            .OrderByDescending(e => e.Id)
            .FirstOrDefaultAsync();

        int nextNumber = lastEmployee is not null ? lastEmployee.Id + 1 : 1;
        return $"EMP-{nextNumber:D4}"; // EMP-0001, EMP-0002 ...
    }

    // Override GetByIdAsync to include relations
    public new async Task<Employee?> GetByIdAsync(int id)
        => await _set
            .Include(e => e.Department)
            .Include(e => e.JobPosition)
            .Include(e => e.Manager)
            .Include(e => e.Contracts)
            .FirstOrDefaultAsync(e => e.Id == id);
}

// ─── Department Repository ────────────────────────────
public class DepartmentRepository : Repository<Department>, IDepartmentRepository
{
    public DepartmentRepository(HRDbContext context) : base(context) { }

    public async Task<bool> HasEmployeesAsync(int departmentId)
        => await _context.Employees.AnyAsync(e => e.DepartmentId == departmentId);

    public new async Task<IEnumerable<Department>> GetAllAsync()
        => await _set
            .Include(d => d.Manager)
            .Include(d => d.Employees)
            .ToListAsync();
}

// ─── Leave Repository ─────────────────────────────────
public class LeaveRepository : Repository<LeaveRequest>, ILeaveRepository
{
    public LeaveRepository(HRDbContext context) : base(context) { }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(int employeeId)
        => await _set
            .Include(l => l.Employee)
            .Include(l => l.ApprovedBy)
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<LeaveRequest>> GetPendingRequestsAsync()
        => await _set
            .Include(l => l.Employee)
            .Where(l => l.Status == LeaveStatus.Pending)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync();

    public async Task<int> GetApprovedDaysAsync(int employeeId, LeaveType type, int year)
        => await _set
            .Where(l =>
                l.EmployeeId == employeeId &&
                l.Type == type &&
                l.Status == LeaveStatus.Approved &&
                l.StartDate.Year == year)
            .SumAsync(l => l.TotalDays);
}

// ─── Attendance Repository ────────────────────────────
public class AttendanceRepository : Repository<AttendanceRecord>, IAttendanceRepository
{
    public AttendanceRepository(HRDbContext context) : base(context) { }

    public async Task<AttendanceRecord?> GetTodayRecordAsync(int employeeId)
        => await _set.FirstOrDefaultAsync(a =>
            a.EmployeeId == employeeId &&
            a.Date.Date == DateTime.UtcNow.Date);

    public async Task<IEnumerable<AttendanceRecord>> GetByEmployeeAndDateRangeAsync(
        int employeeId, DateTime startDate, DateTime endDate)
        => await _set
            .Include(a => a.Employee)
            .Where(a =>
                a.EmployeeId == employeeId &&
                a.Date >= startDate &&
                a.Date <= endDate)
            .OrderBy(a => a.Date)
            .ToListAsync();
}

// ─── User Repository ──────────────────────────────────
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(HRDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username)
        => await _set.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<User?> GetByEmailAsync(string email)
        => await _set.FirstOrDefaultAsync(u => u.Email == email);
}