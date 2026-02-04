using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using HRSystem.DataAccess;
using HRSystem.DataAccess.Repositories;
using HRSystem.Core.Interfaces;
using HRSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Add Controllers ──────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ─── Add Swagger (Swashbuckle 6.5.0) ─────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HRSystem API",
        Version = "v1",
        Description = "API for HR System"
    });
});

// ─── Database ────────────────────────────────────
builder.Services.AddDbContext<HRDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    );
});

// ─── JWT Authentication ─────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-secret-key-minimum-32-chars!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "HRSystem";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "HRSystemAPI";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ─── Dependency Injection ────────────────────────
// Repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<LeaveService>();
builder.Services.AddScoped<AuthService>();

// ─── CORS ───────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ─── Build App ──────────────────────────────────
var app = builder.Build();

// ─── Middleware ─────────────────────────────────
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ─── Swagger Middleware (Development Only) ──────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HRSystem API V1");
        c.RoutePrefix = string.Empty; // لو عايز Swagger يفتح على root
    });
}

// ─── Map Controllers ────────────────────────────
app.MapControllers();

// ─── Run App ───────────────────────────────────
app.Run();
