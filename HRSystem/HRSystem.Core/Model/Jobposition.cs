namespace HRSystem.Core.Models;

public class JobPosition
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}