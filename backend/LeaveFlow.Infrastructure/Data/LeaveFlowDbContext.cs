using LeaveFlow.Core.Entities;
using LeaveFlow.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;

namespace LeaveFlow.Infrastructure.Data;

public class LeaveFlowDbContext : DbContext
{
    private readonly IEncryptionService _encryptionService;
    private readonly ICurrentTenantService _currentTenantService;

    public LeaveFlowDbContext(DbContextOptions<LeaveFlowDbContext> options, IEncryptionService encryptionService, ICurrentTenantService currentTenantService)
        : base(options)
    {
        _encryptionService = encryptionService;
        _currentTenantService = currentTenantService;
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();
    public DbSet<PublicHoliday> PublicHolidays => Set<PublicHoliday>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<CountryWorkday> CountryWorkdays => Set<CountryWorkday>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Value converter for encrypted fields
        var encryptedConverter = new ValueConverter<string?, string?>(
            v => v == null ? null : _encryptionService.Encrypt(v),
            v => v == null ? null : _encryptionService.Decrypt(v)
        );

        // Apply Global Query Filters for ITenantEntity
        // This ensures all queries automatically filter by TenantId
        var tenantId = _currentTenantService.TenantId;
        
        // We use a defined method to keep the lambda expression clean
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = SetGlobalQueryFilterMethod.MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { modelBuilder });
            }
        }

        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique(); 
            // Note: In multi-tenancy, email uniqueness might need to be per-tenant OR global. 
            // Usually global for login, so we keep it unique globally for now.
            
            // Encrypted fields
            entity.Property(e => e.Salary).HasConversion(encryptedConverter);
            entity.Property(e => e.PassportNumber).HasConversion(encryptedConverter);
            entity.Property(e => e.Phone).HasConversion(encryptedConverter);
            entity.Property(e => e.Address).HasConversion(encryptedConverter);
            entity.Property(e => e.EmergencyContact).HasConversion(encryptedConverter);
            entity.Property(e => e.NationalId).HasConversion(encryptedConverter);
            entity.Property(e => e.BankAccount).HasConversion(encryptedConverter);

            // Self-referencing relationship for Manager
            entity.HasOne(e => e.Manager)
                .WithMany(e => e.DirectReports)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LeaveRequest configuration
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Encrypted fields
            entity.Property(e => e.MedicalCertificate).HasConversion(encryptedConverter);
            entity.Property(e => e.PersonalNotes).HasConversion(encryptedConverter);

            entity.HasOne(e => e.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.LeaveType)
                .WithMany(t => t.LeaveRequests)
                .HasForeignKey(e => e.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Approver)
                .WithMany()
                .HasForeignKey(e => e.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LeaveType configuration
        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Name should be unique per tenant
            entity.HasIndex(e => new { e.Name, e.TenantId }).IsUnique();
        });

        // LeaveBalance configuration
        modelBuilder.Entity<LeaveBalance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EmployeeId, e.LeaveTypeId, e.Year }).IsUnique();

            entity.HasOne(e => e.Employee)
                .WithMany(e => e.LeaveBalances)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.LeaveType)
                .WithMany(t => t.LeaveBalances)
                .HasForeignKey(e => e.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AuditLog configuration  
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CompanySettings configuration
        modelBuilder.Entity<CompanySettings>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // PublicHoliday configuration
        modelBuilder.Entity<PublicHoliday>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CountryCode, e.Date, e.TenantId }).IsUnique();
        });

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CountryWorkday configuration
        modelBuilder.Entity<CountryWorkday>(entity =>
        {
            entity.HasKey(e => e.CountryCode);
        });

        // Seed data
        // Note: Seeding in OnModelCreating doesn't work well with dynamic TenantIds in Query Filters during runtime interactions
        // We will move seeding to Program.cs entirely for safety and clarity
    }

    // Method to apply the global query filter
    private static readonly MethodInfo SetGlobalQueryFilterMethod = typeof(LeaveFlowDbContext)
        .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetGlobalQueryFilter));

    private void SetGlobalQueryFilter<T>(ModelBuilder builder) where T : class, ITenantEntity
    {
        builder.Entity<T>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _currentTenantService.TenantId;

        // Auto-set TenantId for new entities
        if (tenantId.HasValue)
        {
            foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.TenantId = tenantId.Value;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

