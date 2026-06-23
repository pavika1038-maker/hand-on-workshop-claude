namespace LeaveRequest.Infrastructure.Data.Configurations;

using LeaveRequest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ImportLogConfiguration : IEntityTypeConfiguration<ImportLog>
{
    public void Configure(EntityTypeBuilder<ImportLog> builder)
    {
        builder.ToTable("ImportLogs");
        builder.HasKey(x => x.ImportLogId);

        builder.Property(x => x.FileName).IsRequired().HasMaxLength(255);
        builder.Property(x => x.ImportedBy).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ErrorDetailsJson).IsRequired();
        builder.Property(x => x.IsRolledBack).HasConversion<int>().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).HasColumnType("TEXT");
        builder.Property(x => x.CreatedBy).IsRequired().HasMaxLength(100);
    }
}
