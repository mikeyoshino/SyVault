namespace DigitalVault.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IDocumentRepository Documents { get; }
    IFamilyMemberRepository FamilyMembers { get; }
    IAccountRepository Accounts { get; }
    Task<int> CompleteAsync();
    Task ExecuteInTransactionAsync(Func<Task> action);
}
