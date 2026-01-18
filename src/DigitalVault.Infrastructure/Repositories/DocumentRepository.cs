using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Entities;
using DigitalVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalVault.Infrastructure.Repositories;

public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
{
    public DocumentRepository(ApplicationDbContext context) : base(context)
    {
    }
}
