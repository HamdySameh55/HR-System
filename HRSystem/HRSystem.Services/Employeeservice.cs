using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

using HRSystem.Core.DTOs;
using HRSystem.Core.Interfaces;
using HRSystem.Core.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;


namespace HRSystem.Services;

// ─── Department Service ───────────────────────────────
public class DepartmentService
{
    private readonly IDepartmentRepository _repo;

    public DepartmentService(IDepartmentRepository repo) { _repo = repo; }

    public async Task<IEnumerable<DepartmentResponseDto>> GetAllAsync()
    {
        var depts = await _repo.GetAllAsync();
        return depts.Select(d => new DepartmentResponseDto
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            ManagerName = d.Manager?.FullName,
            EmployeeCount = d.Employees.Count
        });
    }

    public async Task<DepartmentResponseDto?> GetByIdAsync(int id)
    {
        var dept = await _repo.GetByIdAsync(id);
        if (dept is null) return null;
        return new DepartmentResponseDto
        {
            Id = dept.Id,
            Name = dept.Name,
            Description = dept.Description,
            ManagerName = dept.Manager?.FullName,
            EmployeeCount = dept.Employees?.Count ?? 0
        };
    }

    public async Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto)
    {
        var dept = new Department
        {
            Name = dto.Name,
            Description = dto.Description,
            ManagerId = dto.ManagerId
        };
        var created = await _repo.AddAsync(dept);
        return new DepartmentResponseDto
        {
            Id = created.Id,
            Name = created.Name,
            Description = created.Description
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (await _repo.HasEmployeesAsync(id))
            throw new InvalidOperationException("Cannot delete department with employees.");
        return await _repo.DeleteAsync(id);
    }
}

// ─── Leave Service ────────────────────────────────────
public class LeaveService
{
    private readonly ILeaveRepository _repo;
    private readonly IEmployeeRepository _empRepo;

    public LeaveService(ILeaveRepository repo, IEmployeeRepository empRepo)
    {
        _repo = repo;
        _empRepo = empRepo;
    }

    public async Task<LeaveResponseDto> CreateLeaveRequestAsync(CreateLeaveRequestDto dto)
    {
        var employee = await _empRepo.GetByIdAsync(dto.EmployeeId)
            ?? throw new InvalidOperationException("Employee not found.");

        int totalDays = (dto.EndDate - dto.StartDate).Days + 1;

        // Check: Annual leave max 30 days/year
        if (dto.Type == LeaveType.Annual)
        {
            int usedDays = await _repo.GetApprovedDaysAsync(dto.EmployeeId, LeaveType.Annual, dto.StartDate.Year);
            if (usedDays + totalDays > 30)
                throw new InvalidOperationException($"Annual leave limit exceeded. Used: {usedDays}/30 days.");
        }

        var leave = new LeaveRequest
        {
            EmployeeId = dto.EmployeeId,
            Type = dto.Type,
            Status = LeaveStatus.Pending,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalDays = totalDays,
            Reason = dto.Reason
        };

        var created = await _repo.AddAsync(leave);
        return MapToResponse(created);
    }

    public async Task<LeaveResponseDto?> ApproveLeaveAsync(int leaveId, int approvedById)
    {
        var leave = await _repo.GetByIdAsync(leaveId);
        if (leave is null) return null;
        if (leave.Status != LeaveStatus.Pending)
            throw new InvalidOperationException("Only pending leaves can be approved.");

        leave.Status = LeaveStatus.Approved;
        leave.ApprovedById = approvedById;
        leave.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.UpdateAsync(leave);
        return MapToResponse(updated);
    }

    public async Task<LeaveResponseDto?> RejectLeaveAsync(int leaveId)
    {
        var leave = await _repo.GetByIdAsync(leaveId);
        if (leave is null) return null;

        leave.Status = LeaveStatus.Rejected;
        leave.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.UpdateAsync(leave);
        return MapToResponse(updated);
    }

    public async Task<IEnumerable<LeaveResponseDto>> GetPendingRequestsAsync()
    {
        var requests = await _repo.GetPendingRequestsAsync();
        return requests.Select(MapToResponse);
    }

    public async Task<IEnumerable<LeaveResponseDto>> GetByEmployeeAsync(int employeeId)
    {
        var requests = await _repo.GetByEmployeeAsync(employeeId);
        return requests.Select(MapToResponse);
    }

    private static LeaveResponseDto MapToResponse(LeaveRequest l) => new()
    {
        Id = l.Id,
        EmployeeName = l.Employee?.FullName ?? "Unknown",
        Type = l.Type,
        Status = l.Status,
        StartDate = l.StartDate,
        EndDate = l.EndDate,
        TotalDays = l.TotalDays,
        Reason = l.Reason
    };
}

// ─── Auth Service ─────────────────────────────────────
public class AuthService
{
    private readonly IUserRepository _repo;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository repo, IConfiguration config)
    {
        _repo = repo;
        _config = config;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _repo.GetByUsernameAsync(dto.Username);
        if (user is null || !user.IsActive) return null;

        // Verify password hash
        string hashedInput = HashPassword(dto.Password);
        if (user.PasswordHash != hashedInput) return null;

        // Generate JWT Token
        var expiresAt = DateTime.UtcNow.AddHours(24);
        string token = GenerateJwtToken(user, expiresAt);

        return new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Role = user.Role,
            ExpiresAt = expiresAt
        };
    }

    public static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

private string GenerateJwtToken(User user, DateTime expiresAt)
{
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
    );

    var credentials = new SigningCredentials(
        key,
        SecurityAlgorithms.HmacSha256
    );

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role.ToString())
    };

    var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: expiresAt,
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
}