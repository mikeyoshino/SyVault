using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Entities;
using DigitalVault.Infrastructure.Data;

namespace DigitalVault.Infrastructure.Repositories;

public class AccountRepository : GenericRepository<Account>, IAccountRepository
{
    public AccountRepository(ApplicationDbContext context) : base(context)
    {
    }
}
