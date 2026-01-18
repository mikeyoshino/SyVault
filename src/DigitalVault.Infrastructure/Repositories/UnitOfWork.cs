using DigitalVault.Application.Interfaces;
using DigitalVault.Infrastructure.Data;

namespace DigitalVault.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IDocumentRepository Documents { get; private set; }
    public IFamilyMemberRepository FamilyMembers { get; private set; }
    public IAccountRepository Accounts { get; private set; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Documents = new DocumentRepository(_context);
        FamilyMembers = new FamilyMemberRepository(_context);
        Accounts = new AccountRepository(_context);
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        // Use the execution strategy to handle retries with transactions
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(
            state: action,
            operation: async (context, action, ct) =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(ct);
                try
                {
                    await action();
                    await transaction.CommitAsync(ct);
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            },
            verifySucceeded: null,
            cancellationToken: default);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
