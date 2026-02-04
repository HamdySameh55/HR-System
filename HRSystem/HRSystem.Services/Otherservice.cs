using HRSystem.Core.DTOs;
using HRSystem.Core.Interfaces;
using HRSystem.Core.Models;

namespace HRSystem.Services;

public class EmployeeService
{
    private readonly IEmployeeRepository _repo;
    private readonly IDepartmentRepository _deptRepo;

    public EmployeeService(IEmployeeRepository repo, IDepartmentRepository deptRepo)
    {
        _repo = repo;
        _deptRepo = deptRepo;
    }

    // ─── Get All ──────────────────────────────────────
    public async Task<IEnumerable<EmployeeResponseDto>> GetAllAsync()
    {
        var employees = await _repo.GetAllAsync();
        return employees.Select(MapToResponse);
    }

    // ─── Get By Id ────────────────────────────────────
    public async Task<EmployeeResponseDto?> GetByIdAsync(int id)
    {
        var employee = await _repo.GetByIdAsync(id);
        return employee is not null ? MapToResponse(employee) : null;
    }

    // ─── Get By Department ────────────────────────────
    public async Task<IEnumerable<EmployeeResponseDto>> GetByDepartmentAsync(int departmentId)
    {
        var employees = await _repo.GetByDepartmentAsync(departmentId);
        return employees.Select(MapToResponse);
    }

    // ─── Create ───────────────────────────────────────
    public async Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto)
    {
        // Validation
        var dept = await _deptRepo.GetByIdAsync(dto.DepartmentId)
            ?? throw new InvalidOperationException($"Department {dto.DepartmentId} not found.");

        if (dto.ManagerId.HasValue)
        {
            var manager = await _repo.GetByIdAsync(dto.ManagerId.Value)
                ?? throw new InvalidOperationException($"Manager {dto.ManagerId} not found.");
        }

        // Generate Employee Number
        string empNumber = await _repo.GenerateEmployeeNumberAsync();

        // Map DTO → Entity
        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            NationalId = dto.NationalId,
            Address = dto.Address,
            EmployeeNumber = empNumber,
            HireDate = dto.HireDate,
            DepartmentId = dto.DepartmentId,
            JobPositionId = dto.JobPositionId,
            ManagerId = dto.ManagerId,
            BaseSalary = dto.BaseSalary
        };

        var created = await _repo.AddAsync(employee);
        return MapToResponse(created);
    }

    // ─── Update ───────────────────────────────────────
    public async Task<EmployeeResponseDto?> UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        var employee = await _repo.GetByIdAsync(id);
        if (employee is null) return null;

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.Phone = dto.Phone;
        employee.DateOfBirth = dto.DateOfBirth;
        employee.Gender = dto.Gender;
        employee.NationalId = dto.NationalId;
        employee.Address = dto.Address;
        employee.HireDate = dto.HireDate;
        employee.DepartmentId = dto.DepartmentId;
        employee.JobPositionId = dto.JobPositionId;
        employee.ManagerId = dto.ManagerId;
        employee.BaseSalary = dto.BaseSalary;
        employee.Status = dto.Status;
        employee.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.UpdateAsync(employee);
        return MapToResponse(updated);
    }

    // ─── Delete ───────────────────────────────────────
    public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);

    // ─── Mapping ──────────────────────────────────────
    private static EmployeeResponseDto MapToResponse(Employee e)
    {
        return new EmployeeResponseDto
        {
            Id = e.Id,
            EmployeeNumber = e.EmployeeNumber,
            FullName = e.FullName,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Phone = e.Phone,
            DateOfBirth = e.DateOfBirth,
            Gender = e.Gender,
            HireDate = e.HireDate,
            Status = e.Status,
            DepartmentName = e.Department?.Name ?? "N/A",
            JobTitle = e.JobPosition?.Title ?? "N/A",
            ManagerName = e.Manager?.FullName,
            BaseSalary = e.BaseSalary
        };
    }
}