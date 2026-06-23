namespace LeaveRequest.Infrastructure.Data.Configurations;

using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CancelRequestConfiguration : IEntityTypeConfiguration<CancelRequest>
{
    public void Configure(EntityTypeBuilder<CancelRequest> builder)
    {
        builder.ToTable("CancelRequests");
        builder.HasKey(x => x.CancelRequestId);

        builder.Property(x => x.CancelRequestRef).IsRequired().HasMaxLength(30);
        builder.Property(x => x.EmployeeId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(CancelRequestStatus.Pending);

        builder.Property(x => x.SlaDeadline).HasColumnType("TEXT").IsRequired();
        builder.Property(x => x.SlaReminderSentAt).HasColumnType("TEXT");
        builder.Property(x => x.SlaEscalatedAt).HasColumnType("TEXT");

        builder.Property(x => x.CreatedAt).HasColumnType("TEXT").IsRequired();
        builder.Property(x => x.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(x => x.UpdatedAt).HasColumnType("TEXT");
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

        builder.HasOne(x => x.LeaveRequest)
            .WithMany()
            .HasForeignKey(x => x.LeaveRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CancelRequestRef).IsUnique();
        builder.HasIndex(x => new { x.Status, x.IsDeleted, x.SlaDeadline });
    }
}
