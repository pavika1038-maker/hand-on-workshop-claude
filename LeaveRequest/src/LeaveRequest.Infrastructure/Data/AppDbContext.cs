using LeaveRequest.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Entities = LeaveRequest.Domain.Entities;

namespace LeaveRequest.Infrastructure.Data;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Entities.LeaveType> LeaveTypes => Set<Entities.LeaveType>();
    public DbSet<Entities.LeaveRequest> LeaveRequests => Set<Entities.LeaveRequest>();
    public DbSet<Entities.LeaveBalance> LeaveBalances => Set<Entities.LeaveBalance>();
    public DbSet<Entities.Employee> Employees => Set<Entities.Employee>();
    public DbSet<Entities.NotificationLog> NotificationLogs => Set<Entities.NotificationLog>();
    public DbSet<Entities.ImportLog> ImportLogs => Set<Entities.ImportLog>();
    public DbSet<Entities.CancelRequest> CancelRequests => Set<Entities.CancelRequest>();
    public DbSet<Entities.ApprovalHistory> ApprovalHistories => Set<Entities.ApprovalHistory>();
    public DbSet<Entities.Attachment> Attachments => Set<Entities.Attachment>();

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await base.SaveChangesAsync(ct);

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
        => new EfCoreTransaction(await Database.BeginTransactionAsync(ct));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            return;

        optionsBuilder.UseSqlite(o => o.MigrationsAssembly("LeaveRequest.Infrastructure"));
    }
}
