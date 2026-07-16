namespace LeaveRequest.Infrastructure.Data.Configurations;

using LeaveRequest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");
        builder.HasKey(x => x.AttachmentId);

        builder.Property(x => x.FileName).IsRequired().HasMaxLength(260);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.FileSize).IsRequired();
        builder.Property(x => x.Content).HasColumnType("BLOB").IsRequired();

        builder.Property(x => x.UploadedBy).IsRequired().HasMaxLength(100);
        builder.Property(x => x.CreatedAt).HasColumnType("TEXT").IsRequired();

        builder.HasOne(x => x.LeaveRequest)
            .WithMany()
            .HasForeignKey(x => x.LeaveRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.LeaveRequestId);
    }
}
