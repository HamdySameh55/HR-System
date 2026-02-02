using Microsoft.EntityFrameworkCore;
using HRSystem.Core.Models;

namespace HRSystem.DataAccess;

public class HRDbContext : DbContext
{
    public HRDbContext(DbContextOptions<HRDbContext> options) : base(options) { }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<JobPosition> JobPositions { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<AttendanceRecord> Attendances { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ─── Employee ─────────────────────────────────
        mb.Entity<Employee>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.EmployeeNumber).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.EmployeeNumber).IsUnique();
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(255);
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.NationalId).HasMaxLength(50);
            e.Property(x => x.BaseSalary).HasPrecision(18, 2);

            e.HasOne(x => x.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(x => x.DepartmentId);

            e.HasOne(x => x.JobPosition)
                .WithMany(j => j.Employees)
                .HasForeignKey(x => x.JobPositionId);

            e.HasOne(x => x.Manager)
                .WithMany()
                .HasForeignKey(x => x.ManagerId)
                .IsRequired(false);
        });

        // ─── Department ───────────────────────────────
        mb.Entity<Department>(d =>
        {
            d.HasKey(x => x.Id);
            d.Property(x => x.Name).HasMaxLength(100).IsRequired();
            d.HasIndex(x => x.Name).IsUnique();

            d.HasOne(x => x.Manager)
                .WithMany()
                .HasForeignKey(x => x.ManagerId)
                .IsRequired(false);
        });

        // ─── JobPosition ──────────────────────────────
        mb.Entity<JobPosition>(j =>
        {
            j.HasKey(x => x.Id);
            j.Property(x => x.Title).HasMaxLength(150).IsRequired();
            j.Property(x => x.MinSalary).HasPrecision(18, 2);
            j.Property(x => x.MaxSalary).HasPrecision(18, 2);
        });

        // ─── Contract ─────────────────────────────────
        mb.Entity<Contract>(c =>
        {
            c.HasKey(x => x.Id);
            c.Property(x => x.Salary).HasPrecision(18, 2);
            c.HasOne(x => x.Employee)
                .WithMany(e => e.Contracts)
                .HasForeignKey(x => x.EmployeeId);
        });

        // ─── LeaveRequest ─────────────────────────────
        mb.Entity<LeaveRequest>(l =>
        {
            l.HasKey(x => x.Id);
            l.HasOne(x => x.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(x => x.EmployeeId);
            l.HasOne(x => x.ApprovedBy)
                .WithMany()
                .HasForeignKey(x => x.ApprovedById)
                .IsRequired(false);
        });

        // ─── Attendance ───────────────────────────────
        mb.Entity<AttendanceRecord>(a =>
        {
            a.HasKey(x => x.Id);
            a.HasOne(x => x.Employee)
                .WithMany(e => e.Attendances)
                .HasForeignKey(x => x.EmployeeId);
            a.HasIndex(x => new { x.EmployeeId, x.Date }).IsUnique();
        });

        // ─── User ─────────────────────────────────────
        mb.Entity<User>(u =>
        {
            u.HasKey(x => x.Id);
            u.Property(x => x.Username).HasMaxLength(100).IsRequired();
            u.HasIndex(x => x.Username).IsUnique();
            u.Property(x => x.Email).HasMaxLength(255).IsRequired();
            u.HasIndex(x => x.Email).IsUnique();
        });
    }
}
