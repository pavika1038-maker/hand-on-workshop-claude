namespace LeaveRequest.Infrastructure.Data.Configurations;

using LeaveRequest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalances");

        builder.HasKey(x => x.LeaveBalanceId);
        builder.Property(x => x.LeaveBalanceId).HasConversion<string>();

        builder.Property(x => x.EmployeeId).IsRequired().HasMaxLength(50);

        // decimal → TEXT to preserve precision (OI-002)
        builder.Property(x => x.EntitledDays).HasColumnType("TEXT");
        builder.Property(x => x.UsedDays).HasColumnType("TEXT");
        builder.Property(x => x.PendingDays).HasColumnType("TEXT");
        builder.Property(x => x.CarriedForwardDays).HasColumnType("TEXT");

        // RemainingDays is computed — not persisted
        builder.Ignore(x => x.RemainingDays);

        builder.Property(x => x.IsDeleted).HasConversion<int>().HasDefaultValue(false);

        builder.Property(x => x.CreatedAt).HasColumnType("TEXT");
        builder.Property(x => x.UpdatedAt).HasColumnType("TEXT");
        builder.Property(x => x.DeletedAt).HasColumnType("TEXT");
        builder.Property(x => x.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.DeletedBy).HasMaxLength(100);

        builder.HasIndex(x => new { x.EmployeeId, x.LeaveTypeId, x.LeaveYear })
            .IsUnique()
            .HasDatabaseName("UQ_LeaveBalances_Employee_Type_Year");

        builder.HasQueryFilter(x => !x.IsDeleted);

        // FK: Employee
        builder.HasOne(x => x.Employee)
            .WithMany(x => x.LeaveBalances)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK: LeaveType (configured in LeaveTypeConfiguration)

        // ── Seed Data: EMP001 สมชาย (year 2026) ───────────────────────────────
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            // LeaveTypeId=1: ลาพักร้อน (ANNUAL) — entitled 10 วัน
            new LeaveBalance
            {
                LeaveBalanceId     = new Guid("10000001-0000-0000-0000-000000000001"),
                EmployeeId         = "EMP001",
                LeaveTypeId        = 1,
                LeaveYear          = 2026,
                EntitledDays       = 10m,
                UsedDays           = 2m,
                PendingDays        = 0m,
                CarriedForwardDays = 0m,
                IsDeleted          = false,
                CreatedAt          = now,
                CreatedBy          = "SYSTEM",
            },
            // LeaveTypeId=2: ลาป่วย (SICK) — unlimited แต่ track ไว้ 30 วัน
            new LeaveBalance
            {
                LeaveBalanceId     = new Guid("10000001-0000-0000-0000-000000000002"),
                EmployeeId         = "EMP001",
                LeaveTypeId        = 2,
                LeaveYear          = 2026,
                EntitledDays       = 30m,
                UsedDays           = 3m,
                PendingDays        = 0m,
                CarriedForwardDays = 0m,
                IsDeleted          = false,
                CreatedAt          = now,
                CreatedBy          = "SYSTEM",
            },
            // LeaveTypeId=3: ลากิจ (PERSONAL) — 3 วัน
            new LeaveBalance
            {
                LeaveBalanceId     = new Guid("10000001-0000-0000-0000-000000000003"),
                EmployeeId         = "EMP001",
                LeaveTypeId        = 3,
                LeaveYear          = 2026,
                EntitledDays       = 3m,
                UsedDays           = 1m,
                PendingDays        = 0m,
                CarriedForwardDays = 0m,
                IsDeleted          = false,
                CreatedAt          = now,
                CreatedBy          = "SYSTEM",
            },
            // LeaveTypeId=4: ลาคลอด (MATERNITY) — 98 วัน
            new LeaveBalance
            {
                LeaveBalanceId     = new Guid("10000001-0000-0000-0000-000000000004"),
                EmployeeId         = "EMP001",
                LeaveTypeId        = 4,
                LeaveYear          = 2026,
                EntitledDays       = 98m,
                UsedDays           = 0m,
                PendingDays        = 0m,
                CarriedForwardDays = 0m,
                IsDeleted          = false,
                CreatedAt          = now,
                CreatedBy          = "SYSTEM",
            },

            // ── EMP002 (วิภา / Manager) ───────────────────────────────────────────
            new LeaveBalance { LeaveBalanceId = new Guid("10000002-0000-0000-0000-000000000001"), EmployeeId = "EMP002", LeaveTypeId = 1, LeaveYear = 2026, EntitledDays = 10m, UsedDays = 3m, PendingDays = 3m, CarriedForwardDays = 0m, IsDeleted = false, CreatedAt = now, CreatedBy = "SYSTEM" },
            new LeaveBalance { LeaveBalanceId = new Guid("10000002-0000-0000-0000-000000000002"), EmployeeId = "EMP002", LeaveTypeId = 2, LeaveYear = 2026, EntitledDays = 30m, UsedDays = 1m, PendingDays = 0m, CarriedForwardDays = 0m, IsDeleted = false, CreatedAt = now, CreatedBy = "SYSTEM" },
            new LeaveBalance { LeaveBalanceId = new Guid("10000002-0000-0000-0000-000000000003"), EmployeeId = "EMP002", LeaveTypeId = 3, LeaveYear = 2026, EntitledDays = 3m, UsedDays = 0m, PendingDays = 0m, CarriedForwardDays = 0m, IsDeleted = false, CreatedAt = now, CreatedBy = "SYSTEM" },

            // ── EMP003 (นันทา / HR) ───────────────────────────────────────────────
            new LeaveBalance { LeaveBalanceId = new Guid("10000003-0000-0000-0000-000000000001"), EmployeeId = "EMP003", LeaveTypeId = 1, LeaveYear = 2026, EntitledDays = 10m, UsedDays = 5m, PendingDays = 0m, CarriedForwardDays = 2m, IsDeleted = false, CreatedAt = now, CreatedBy = "SYSTEM" },
            new LeaveBalance { LeaveBalanceId = new Guid("10000003-0000-0000-0000-000000000002"), EmployeeId = "EMP003", LeaveTypeId = 2, LeaveYear = 2026, EntitledDays = 30m, UsedDays = 0m, PendingDays = 0m, CarriedForwardDays = 0m, IsDeleted = false, CreatedAt = now, CreatedBy = "SYSTEM" },
            new LeaveBalance { LeaveBalanceId = new Guid("10000003-0000-0000-0000-000000000003"), EmployeeId = "EMP003", LeaveTypeId = 3, LeaveYear = 2026, EntitledDays = 3m, UsedDays = 2m, PendingDays = 0m, CarriedForwardDays = 0m, IsDeleted = false, CreatedAt = now, CreatedBy = "SYSTEM" }
        );
    }
}
