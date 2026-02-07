using System.Text;
using LeaveFlow.Core.Entities;
using LeaveFlow.Core.Interfaces;
using LeaveFlow.Infrastructure.Data;
using LeaveFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "LeaveFlow-Super-Secret-Key-2024-Minimum-32-Characters!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LeaveFlow";
var encryptionKey = builder.Configuration["Encryption:Key"] ?? "LeaveFlow-AES-256-Encryption-Key-2024!";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=leaveflow;Username=postgres;Password=postgres";

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor(); // Required for CurrentTenantService

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LeaveFlow API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Encryption service (singleton - key doesn't change)
builder.Services.AddSingleton<IEncryptionService>(new EncryptionService(encryptionKey));

// Tenant Service
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();

// Database context
builder.Services.AddDbContext<LeaveFlowDbContext>((serviceProvider, options) =>
{
    options.UseNpgsql(connectionString);
});

// Auth service
builder.Services.AddScoped<IAuthService>(sp =>
{
    var dbContext = sp.GetRequiredService<LeaveFlowDbContext>();
    return new AuthService(dbContext, jwtSecret, jwtIssuer);
});

// HttpClient for external APIs (holidays)
builder.Services.AddHttpClient();

// Holiday service
builder.Services.AddScoped<IHolidayService, HolidayService>();

// File service
builder.Services.AddScoped<IFileService, FileService>();

// Notification service
builder.Services.AddScoped<INotificationService, NotificationService>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var dbContext = services.GetRequiredService<LeaveFlowDbContext>();
        
        // Wipe database as requested by user
        Console.WriteLine("Wiping database...");
        await dbContext.Database.EnsureDeletedAsync();
        
        Console.WriteLine("Ensuring database created...");
        await dbContext.Database.EnsureCreatedAsync(); // This creates tables but NO SEED DATA anymore

        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        
        // 1. Seed Company Settings (Manual)
        if (!dbContext.CompanySettings.Any())
        {
             Console.WriteLine("Seeding Company Settings...");
             dbContext.CompanySettings.Add(new CompanySettings 
            { 
                Id = tenantId, // Use TenantId as ID for simplicity in single-tenant feel
                TenantId = tenantId,
                CompanyName = "LeaveFlow",
                DefaultDailyWorkingHours = 8.0m,
                MinLeaveHours = 0.5m,
                DefaultCountryCode = "AE",
                Sunday = true, Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = false, Saturday = false
            });
            await dbContext.SaveChangesAsync();
        }

        // 2. Seed Leave Types (Manual)
        if (!dbContext.LeaveTypes.Any())
        {
            Console.WriteLine("Seeding Leave Types...");
            var types = new[] {
                new LeaveType { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), TenantId = tenantId, Name = "Annual", Description = "Annual vacation leave", DefaultHoursPerYear = 168, ApplicableGender = LeaveFlow.Core.Enums.ApplicableGender.All, ColorCode = "#059669" },
                new LeaveType { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), TenantId = tenantId, Name = "Sick", Description = "Sick leave", DefaultHoursPerYear = 112, RequiresDocument = true, ApplicableGender = LeaveFlow.Core.Enums.ApplicableGender.All, ColorCode = "#ef4444" },
                new LeaveType { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), TenantId = tenantId, Name = "Personal", Description = "Personal leave", DefaultHoursPerYear = 40, ApplicableGender = LeaveFlow.Core.Enums.ApplicableGender.All, ColorCode = "#8b5cf6" },
                new LeaveType { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), TenantId = tenantId, Name = "Maternity", Description = "Maternity leave", DefaultHoursPerYear = 720, ApplicableGender = LeaveFlow.Core.Enums.ApplicableGender.Female, ColorCode = "#ec4899" },
                new LeaveType { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), TenantId = tenantId, Name = "Paternity", Description = "Paternity leave", DefaultHoursPerYear = 40, ApplicableGender = LeaveFlow.Core.Enums.ApplicableGender.Male, ColorCode = "#3b82f6" }
            };
            dbContext.LeaveTypes.AddRange(types);
            await dbContext.SaveChangesAsync();
        }

        // 3. Seed Admin User
        var adminId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var currentYear = 2026; 

        if (!dbContext.Employees.Any(e => e.Email == "admin@leaveflow.com"))
        {
            Console.WriteLine("Seeding Admin User...");
            // Use raw SQL to bypass query filters if needed, but since we are seeding, context is fresh.
            // IMPORTANT: If we use Add(), SaveChanges will OVERRIDE TenantId with null/empty from CurrentTenantService (which is null in console app).
            // We need to temporarily set the tenant in the service OR manually set it if our logic allows.
            // Our logic: "if (tenantId.HasValue) ... entry.Entity.TenantId = tenantId.Value;"
            // Since CurrentTenantService returns null here, we can manually set TenantId on the entity and it won't be overwritten.
            
            dbContext.Employees.Add(new LeaveFlow.Core.Entities.Employee
            {
                Id = adminId,
                TenantId = tenantId, // Manual assignment
                Email = "admin@leaveflow.com",
                FirstName = "System",
                LastName = "Admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = LeaveFlow.Core.Enums.UserRole.SuperAdmin,
                Gender = LeaveFlow.Core.Enums.Gender.Male,
                Department = "HR",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CountryCode = "AE",
                DailyWorkingHours = 8.0m,
                WorkingDays = 31 
            });
            await dbContext.SaveChangesAsync();
            Console.WriteLine("Admin user created.");
        }

        // 4. Seed Balances
        if (!dbContext.LeaveBalances.Any(b => b.EmployeeId == adminId && b.Year == currentYear))
        {
            Console.WriteLine("Seeding initial leave balances for admin...");
            var leaveTypes = await dbContext.LeaveTypes.IgnoreQueryFilters().Where(t => t.TenantId == tenantId).ToListAsync();
            var adminDailyHours = 8.0m;

            foreach (var type in leaveTypes)
            {
                dbContext.LeaveBalances.Add(new LeaveBalance
                {
                    TenantId = tenantId, // Manual assignment
                    EmployeeId = adminId,
                    LeaveTypeId = type.Id,
                    Year = currentYear,
                    TotalHours = (type.DefaultHoursPerYear / 8.0m) * adminDailyHours,
                    UsedHours = 0,
                    PendingHours = 0,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await dbContext.SaveChangesAsync();
        }
        
        Console.WriteLine("Seeding complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
    }
}

app.Run();

