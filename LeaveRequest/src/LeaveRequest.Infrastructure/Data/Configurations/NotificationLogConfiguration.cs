namespace LeaveRequest.Infrastructure.Data.Configurations;

using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("NotificationLogs");

        builder.HasKey(n => n.NotificationLogId);

        builder.Property(n => n.EventType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(n => n.CloudEventType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.CorrelationId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(n => n.RecipientsJson)
            .HasColumnType("TEXT")
            .IsRequired();

        builder.Property(n => n.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.FailureReason)
            .HasMaxLength(1000);

        builder.Property(n => n.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(n => n.LeaveRequestId);
        builder.HasIndex(n => n.CancelRequestId);
        builder.HasIndex(n => new { n.NotificationLogId, n.Status });
    }
}
