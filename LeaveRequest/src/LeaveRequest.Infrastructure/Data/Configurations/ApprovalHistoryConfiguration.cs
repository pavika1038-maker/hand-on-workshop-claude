namespace LeaveRequest.Infrastructure.Data.Configurations;

using LeaveRequest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ApprovalHistoryConfiguration : IEntityTypeConfiguration<ApprovalHistory>
{
    public void Configure(EntityTypeBuilder<ApprovalHistory> builder)
    {
        builder.ToTable("ApprovalHistories");
        builder.HasKey(a => a.ApprovalHistoryId);

        builder.Property(a => a.ApproverId).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Action).HasConversion<int>();
        builder.Property(a => a.Reason).HasMaxLength(1000);
        builder.Property(a => a.CreatedBy).HasMaxLength(100).IsRequired();

        builder.HasOne(a => a.LeaveRequest)
            .WithMany()
            .HasForeignKey(a => a.LeaveRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.CancelRequest)
            .WithMany()
            .HasForeignKey(a => a.CancelRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.LeaveRequestId);
        builder.HasIndex(a => a.CancelRequestId);
    }
}
