namespace LeaveRequest.Infrastructure.Data.Configurations;

using LeaveRequest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("LeaveTypes");

        builder.HasKey(x => x.LeaveTypeId);
        builder.Property(x => x.LeaveTypeId).ValueGeneratedOnAdd();

        builder.Property(x => x.TypeCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.TypeNameTh)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.TypeNameEn)
            .IsRequired()
            .HasMaxLength(100);

        // SQLite stores decimal as REAL — precision limited to ~15 significant digits
        builder.Property(x => x.MaxDaysPerYear)
            .HasColumnType("REAL");

        builder.Property(x => x.IsAvailableForOutsource)
            .HasConversion<int>(); // bool → 0/1 INTEGER

        builder.Property(x => x.RequiresMedicalCert)
            .HasConversion<int>();

        builder.Property(x => x.IsDeleted)
            .HasConversion<int>()
            .HasDefaultValue(false);

        // DateTime stored as TEXT ISO 8601 in SQLite
        builder.Property(x => x.CreatedAt).HasColumnType("TEXT");
        builder.Property(x => x.UpdatedAt).HasColumnType("TEXT");
        builder.Property(x => x.DeletedAt).HasColumnType("TEXT");

        builder.Property(x => x.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.DeletedBy).HasMaxLength(100);

        builder.HasIndex(x => x.TypeCode).IsUnique().HasDatabaseName("UQ_LeaveTypes_TypeCode");

        // Global query filter: exclude soft-deleted records
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Seed Data
        builder.HasData(
            new LeaveType
            {
                LeaveTypeId = 1,
                TypeCode = "ANNUAL",
                TypeNameTh = "ลาพักร้อน",
                TypeNameEn = "Annual Leave",
                MaxDaysPerYear = 10,
                RequiresMedicalCert = false,
                IsAvailableForOutsource = false,
                IsDeleted = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "SYSTEM"
            },
            new LeaveType
            {
                LeaveTypeId = 2,
                TypeCode = "SICK",
                TypeNameTh = "ลาป่วย",
                TypeNameEn = "Sick Leave",
                MaxDaysPerYear = 30,
                RequiresMedicalCert = true,
                IsAvailableForOutsource = true,
                IsDeleted = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "SYSTEM"
            },
            new LeaveType
            {
                LeaveTypeId = 3,
                TypeCode = "PERSONAL",
                TypeNameTh = "ลากิจ",
                TypeNameEn = "Personal Leave",
                MaxDaysPerYear = 3,
                RequiresMedicalCert = false,
                IsAvailableForOutsource = false,
                IsDeleted = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "SYSTEM"
            },
            new LeaveType
            {
                LeaveTypeId = 4,
                TypeCode = "MATERNITY",
                TypeNameTh = "ลาคลอด",
                TypeNameEn = "Maternity Leave",
                MaxDaysPerYear = 98,
                RequiresMedicalCert = true,
                IsAvailableForOutsource = false,
                IsDeleted = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "SYSTEM"
            }
        );

        // Navigation properties
        builder.HasMany(x => x.LeaveRequests)
            .WithOne(x => x.LeaveType)
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.LeaveBalances)
            .WithOne(x => x.LeaveType)
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
