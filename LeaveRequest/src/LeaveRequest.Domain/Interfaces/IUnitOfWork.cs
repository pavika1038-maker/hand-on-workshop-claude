namespace LeaveRequest.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);
}
