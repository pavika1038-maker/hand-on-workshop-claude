namespace LeaveRequest.Infrastructure.Data.Configurations;

using LeaveRequest.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entities = LeaveRequest.Domain.Entities;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<Entities.LeaveRequest>
{
    public void Configure(EntityTypeBuilder<Entities.LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");

        builder.HasKey(x => x.LeaveRequestId);
        builder.Property(x => x.LeaveRequestId).HasConversion<string>();

        builder.Property(x => x.LeaveRequestRef).IsRequired().HasMaxLength(30);
        builder.Property(x => x.EmployeeId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.HalfDayPeriod).HasMaxLength(2);
        builder.Property(x => x.ApprovedBy).HasMaxLength(50);
        builder.Property(x => x.RejectedBy).HasMaxLength(50);
        builder.Property(x => x.RejectionReason).HasMaxLength(500);

        // decimal → TEXT to preserve precision (OI-002 resolved per class diagram)
        builder.Property(x => x.DurationDays).HasColumnType("TEXT");

        // enum → int
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(LeaveStatus.Pending);

        // bool → int
        builder.Property(x => x.IsHalfDay).HasConversion<int>().HasDefaultValue(false);
        builder.Property(x => x.IsDeleted).HasConversion<int>().HasDefaultValue(false);

        // DateOnly → TEXT
        builder.Property(x => x.StartDate).HasColumnType("TEXT");
        builder.Property(x => x.EndDate).HasColumnType("TEXT");

        // DateTime → TEXT (ISO 8601)
        builder.Property(x => x.CreatedAt).HasColumnType("TEXT");
        builder.Property(x => x.UpdatedAt).HasColumnType("TEXT");
        builder.Property(x => x.DeletedAt).HasColumnType("TEXT");
        builder.Property(x => x.ApprovedAt).HasColumnType("TEXT");
        builder.Property(x => x.RejectedAt).HasColumnType("TEXT");
        builder.Property(x => x.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.DeletedBy).HasMaxLength(100);

        builder.HasIndex(x => x.LeaveRequestRef).IsUnique().HasDatabaseName("UQ_LeaveRequests_Ref");
        builder.HasIndex(x => new { x.EmployeeId, x.Status }).HasDatabaseName("IX_LeaveRequests_Employee_Status");

        builder.HasQueryFilter(x => !x.IsDeleted);

        // FK: Employee
        builder.HasOne(x => x.Employee)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK: LeaveType (configured in LeaveTypeConfiguration via HasMany.LeaveRequests)

        // ── Seed Data (Demo) ──────────────────────────────────────────────────────
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var approved1At = new DateTime(2026, 2, 3, 9, 0, 0, DateTimeKind.Utc);
        var rejected1At = new DateTime(2026, 3, 6, 10, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            // EMP001 — อนุมัติแล้ว (ลาพักร้อน)
            new Entities.LeaveRequest
            {
                LeaveRequestId  = new Guid("A0000001-0000-0000-0000-000000000001"),
                LeaveRequestRef = "LR-2026-00001",
                EmployeeId      = "EMP001",
                LeaveTypeId     = 1,
                StartDate       = new DateOnly(2026, 2, 3),
                EndDate         = new DateOnly(2026, 2, 4),
                DurationDays    = 2m,
                IsHalfDay       = false,
                Reason          = "ท่องเที่ยวต่างจังหวัด",
                Status          = LeaveStatus.Approved,
                ApprovedBy      = "EMP002",
                ApprovedAt      = approved1At,
                IsDeleted       = false,
                CreatedAt       = now,
                CreatedBy       = "EMP001",
            },
            // EMP001 — ปฏิเสธ (ลากิจ)
            new Entities.LeaveRequest
            {
                LeaveRequestId  = new Guid("A0000001-0000-0000-0000-000000000002"),
                LeaveRequestRef = "LR-2026-00002",
                EmployeeId      = "EMP001",
                LeaveTypeId     = 3,
                StartDate       = new DateOnly(2026, 3, 6),
                EndDate         = new DateOnly(2026, 3, 6),
                DurationDays    = 1m,
                IsHalfDay       = false,
                Reason          = "ธุระส่วนตัว",
                Status          = LeaveStatus.Rejected,
                RejectedBy      = "EMP002",
                RejectedAt      = rejected1At,
                RejectionReason = "ช่วงนั้นคนในทีมลาหลายคน กรุณาเลื่อนวันได้ไหม",
                IsDeleted       = false,
                CreatedAt       = new DateTime(2026, 3, 4, 8, 0, 0, DateTimeKind.Utc),
                CreatedBy       = "EMP001",
            },
            // EMP001 — รอการอนุมัติ (ลาป่วย)
            new Entities.LeaveRequest
            {
                LeaveRequestId  = new Guid("A0000001-0000-0000-0000-000000000003"),
                LeaveRequestRef = "LR-2026-00003",
                EmployeeId      = "EMP001",
                LeaveTypeId     = 2,
                StartDate       = new DateOnly(2026, 6, 23),
                EndDate         = new DateOnly(2026, 6, 24),
                DurationDays    = 2m,
                IsHalfDay       = false,
                Reason          = "ไม่สบาย มีไข้",
                Status          = LeaveStatus.Pending,
                IsDeleted       = false,
                CreatedAt       = new DateTime(2026, 6, 19, 8, 30, 0, DateTimeKind.Utc),
                CreatedBy       = "EMP001",
            },
            // EMP002 (wipa/Manager) — รอการอนุมัติ (ลาพักร้อน)
            new Entities.LeaveRequest
            {
                LeaveRequestId  = new Guid("A0000002-0000-0000-0000-000000000001"),
                LeaveRequestRef = "LR-2026-00004",
                EmployeeId      = "EMP002",
                LeaveTypeId     = 1,
                StartDate       = new DateOnly(2026, 7, 7),
                EndDate         = new DateOnly(2026, 7, 9),
                DurationDays    = 3m,
                IsHalfDay       = false,
                Reason          = "ไปเที่ยวต่างประเทศ",
                Status          = LeaveStatus.Pending,
                IsDeleted       = false,
                CreatedAt       = new DateTime(2026, 6, 18, 14, 0, 0, DateTimeKind.Utc),
                CreatedBy       = "EMP002",
            }
        );
    }
}
