namespace HRSystem.Core.Models
{
    public class Employee
    {
        public int Id { get; set; }

        
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string? NationalId { get; set; }
        public string? Address { get; set; }
        public string? ProfilePicturePath { get; set; }

       
        public string EmployeeNumber { get; set; } = string.Empty; // HR-001
        public DateTime HireDate { get; set; }
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public int JobPositionId { get; set; }
        public JobPosition JobPosition { get; set; } = null!;

        public int? ManagerId { get; set; }
        public Employee? Manager { get; set; }

    
        public decimal BaseSalary { get; set; }

     
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public ICollection<AttendanceRecord> Attendances { get; set; } = new List<AttendanceRecord>();

       
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
