using HRSystem.Core.Models;

namespace HRSystem.Core.DTOs;

// ─── Employee DTOs ────────────────────────────────────
public class CreateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? NationalId { get; set; }
    public string? Address { get; set; }
    public DateTime HireDate { get; set; }
    public int DepartmentId { get; set; }
    public int JobPositionId { get; set; }
    public int? ManagerId { get; set; }
    public decimal BaseSalary { get; set; }
}

public class UpdateEmployeeDto : CreateEmployeeDto
{
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
}

public class EmployeeResponseDto
{
    public int Id { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public DateTime HireDate { get; set; }
    public EmployeeStatus Status { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public decimal BaseSalary { get; set; }
}

// ─── Department DTOs ──────────────────────────────────
public class CreateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ManagerId { get; set; }
}

public class DepartmentResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ManagerName { get; set; }
    public int EmployeeCount { get; set; }
}

// ─── Leave DTOs ───────────────────────────────────────
public class CreateLeaveRequestDto
{
    public int EmployeeId { get; set; }
    public LeaveType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}

public class LeaveResponseDto
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public LeaveType Type { get; set; }
    public LeaveStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string? Reason { get; set; }
}

// ─── Auth DTOs ────────────────────────────────────────
public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime ExpiresAt { get; set; }
}

// ─── Pagination ───────────────────────────────────────
public class PaginationDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PaginatedResponseDto<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}