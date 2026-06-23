namespace LeaveRequest.Infrastructure.Data.Configurations;

using LeaveRequest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(x => x.EmployeeId);
        builder.Property(x => x.EmployeeId).HasMaxLength(50);

        builder.Property(x => x.EmployeeCode).IsRequired().HasMaxLength(20);
        builder.Property(x => x.FullNameTh).IsRequired().HasMaxLength(150);
        builder.Property(x => x.FullNameEn).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Department).HasMaxLength(100);
        builder.Property(x => x.Position).HasMaxLength(100);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(150);
        builder.Property(x => x.AgencyCompany).HasMaxLength(150);
        builder.Property(x => x.ManagerId).HasMaxLength(50);

        // DateOnly → TEXT in SQLite
        builder.Property(x => x.HireDate).HasColumnType("TEXT");
        builder.Property(x => x.AbcStartDate).HasColumnType("TEXT");
        builder.Property(x => x.LastSyncedAt).HasColumnType("TEXT");

        builder.Property(x => x.EmployeeType).HasConversion<int>();
        builder.Property(x => x.IsActive).HasConversion<int>().HasDefaultValue(true);
        builder.Property(x => x.IsDeleted).HasConversion<int>().HasDefaultValue(false);

        builder.Property(x => x.CreatedAt).HasColumnType("TEXT");
        builder.Property(x => x.UpdatedAt).HasColumnType("TEXT");
        builder.Property(x => x.DeletedAt).HasColumnType("TEXT");
        builder.Property(x => x.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.DeletedBy).HasMaxLength(100);

        builder.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UQ_Employees_Email");
        builder.HasIndex(x => x.EmployeeCode).IsUnique().HasDatabaseName("UQ_Employees_Code");

        builder.HasQueryFilter(x => !x.IsDeleted);

        // Seed Data (username = email, password fixed = "1234" — checked in AuthController)
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new Employee
            {
                EmployeeId   = "EMP001",
                EmployeeCode = "E001",
                FullNameTh   = "สมชาย ใจดี",
                FullNameEn   = "Somchai Jaidee",
                Department   = "Engineering",
                Position     = "Software Engineer",
                Email        = "somchai@abc.com",
                HireDate     = new DateOnly(2022, 1, 15),
                ManagerId    = "EMP002",
                EmployeeType = Domain.Enums.EmployeeType.Regular,
                IsActive     = true,
                IsDeleted    = false,
                CreatedAt    = now,
                CreatedBy    = "SYSTEM"
            },
            new Employee
            {
                EmployeeId   = "EMP002",
                EmployeeCode = "E002",
                FullNameTh   = "วิภา รักงาน",
                FullNameEn   = "Wipa Rakkan",
                Department   = "Engineering",
                Position     = "Engineering Manager",
                Email        = "wipa@abc.com",
                HireDate     = new DateOnly(2019, 6, 1),
                ManagerId    = null,
                EmployeeType = Domain.Enums.EmployeeType.Regular,
                IsActive     = true,
                IsDeleted    = false,
                CreatedAt    = now,
                CreatedBy    = "SYSTEM"
            },
            new Employee
            {
                EmployeeId   = "EMP003",
                EmployeeCode = "E003",
                FullNameTh   = "นันทา มีสุข",
                FullNameEn   = "Nanta Meesuk",
                Department   = "HR",
                Position     = "HR Officer",
                Email        = "nanta@abc.com",
                HireDate     = new DateOnly(2020, 3, 1),
                ManagerId    = "EMP002",
                EmployeeType = Domain.Enums.EmployeeType.Regular,
                IsActive     = true,
                IsDeleted    = false,
                CreatedAt    = now,
                CreatedBy    = "SYSTEM"
            }
        );

        // Self-reference: Manager → Subordinates
        builder.HasOne(x => x.Manager)
            .WithMany(x => x.Subordinates)
            .HasForeignKey(x => x.ManagerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
