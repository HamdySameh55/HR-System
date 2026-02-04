using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRSystem.Core.DTOs;
using HRSystem.Services;

namespace HRSystem.API.Controllers;

// ─── Base Controller ──────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Authorize] // All routes require auth by default
public abstract class BaseController : ControllerBase { }

// ─── Employee Controller ──────────────────────────────
public class EmployeesController : BaseController
{
    private readonly EmployeeService _service;

    public EmployeesController(EmployeeService service) { _service = service; }

    [HttpGet]
    [AllowAnonymous] // or restrict to specific roles
    public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetAll()
    {
        var employees = await _service.GetAllAsync();
        return Ok(employees);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeResponseDto>> GetById(int id)
    {
        var employee = await _service.GetByIdAsync(id);
        if (employee is null) return NotFound($"Employee with ID {id} not found.");
        return Ok(employee);
    }

    [HttpGet("department/{departmentId:int}")]
    public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetByDepartment(int departmentId)
    {
        var employees = await _service.GetByDepartmentAsync(departmentId);
        return Ok(employees);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<EmployeeResponseDto>> Create([FromBody] CreateEmployeeDto dto)
    {
        try
        {
            var employee = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<EmployeeResponseDto>> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var employee = await _service.UpdateAsync(id, dto);
        if (employee is null) return NotFound($"Employee with ID {id} not found.");
        return Ok(employee);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        bool deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound($"Employee with ID {id} not found.");
        return NoContent();
    }
}

// ─── Department Controller ────────────────────────────
public class DepartmentsController : BaseController
{
    private readonly DepartmentService _service;

    public DepartmentsController(DepartmentService service) { _service = service; }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentResponseDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentResponseDto>> GetById(int id)
    {
        var dept = await _service.GetByIdAsync(id);
        if (dept is null) return NotFound();
        return Ok(dept);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<DepartmentResponseDto>> Create([FromBody] CreateDepartmentDto dto)
    {
        var dept = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = dept.Id }, dept);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            bool deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

// ─── Leave Controller ─────────────────────────────────
public class LeavesController : BaseController
{
    private readonly LeaveService _service;

    public LeavesController(LeaveService service) { _service = service; }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<IEnumerable<LeaveResponseDto>>> GetPending()
        => Ok(await _service.GetPendingRequestsAsync());

    [HttpGet("employee/{employeeId:int}")]
    public async Task<ActionResult<IEnumerable<LeaveResponseDto>>> GetByEmployee(int employeeId)
        => Ok(await _service.GetByEmployeeAsync(employeeId));

    [HttpPost]
    public async Task<ActionResult<LeaveResponseDto>> Create([FromBody] CreateLeaveRequestDto dto)
    {
        try
        {
            var leave = await _service.CreateLeaveRequestAsync(dto);
            return CreatedAtAction(nameof(GetByEmployee), new { employeeId = dto.EmployeeId }, leave);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/approve")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<LeaveResponseDto>> Approve(int id)
    {
        // In real app, get approver ID from JWT claims
        int approverIdPlaceholder = 1;
        var leave = await _service.ApproveLeaveAsync(id, approverIdPlaceholder);
        if (leave is null) return NotFound();
        return Ok(leave);
    }

    [HttpPut("{id:int}/reject")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<ActionResult<LeaveResponseDto>> Reject(int id)
    {
        var leave = await _service.RejectLeaveAsync(id);
        if (leave is null) return NotFound();
        return Ok(leave);
    }
}

// ─── Auth Controller ──────────────────────────────────
public class AuthController : ControllerBase
{
    private readonly AuthService _service;

    public AuthController(AuthService service) { _service = service; }

    [HttpPost("api/auth/login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var result = await _service.LoginAsync(dto);
        if (result is null)
            return Unauthorized(new { message = "Invalid username or password." });
        return Ok(result);
    }
}