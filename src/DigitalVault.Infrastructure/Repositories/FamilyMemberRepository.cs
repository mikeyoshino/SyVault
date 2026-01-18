using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Entities;
using DigitalVault.Infrastructure.Data;

namespace DigitalVault.Infrastructure.Repositories;

public class FamilyMemberRepository : GenericRepository<FamilyMember>, IFamilyMemberRepository
{
    public FamilyMemberRepository(ApplicationDbContext context) : base(context)
    {
    }
}
