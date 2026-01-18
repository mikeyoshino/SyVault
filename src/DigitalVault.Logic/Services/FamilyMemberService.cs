using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DigitalVault.Logic.Services;

public class FamilyMemberService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FamilyMemberService> _logger;

    public FamilyMemberService(IUnitOfWork unitOfWork, ILogger<FamilyMemberService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<FamilyMember>> GetFamilyMembersAsync(Guid accountId)
    {
        if (accountId == Guid.Empty)
        {
            // Log or throw?
            // Since this is service layer, throw is better. Controller catches it.
            throw new ArgumentException("Invalid Account ID");
        }

        var members = await _unitOfWork.FamilyMembers.FindAsync(m => m.AccountId == accountId);

        // Auto-seed if empty
        if (!members.Any())
        {
            _logger.LogInformation("No family members found for account {AccountId}. Creating default 'Owner' member.", accountId);
            var owner = await CreateDefaultOwnerAsync(accountId);
            return new List<FamilyMember> { owner };
        }

        return members;
    }

    public async Task<FamilyMember?> GetFamilyMemberAsync(Guid id, Guid accountId)
    {
        var member = await _unitOfWork.FamilyMembers.GetByIdAsync(id);
        if (member != null && member.AccountId != accountId)
        {
            return null; // Access denied
        }
        return member;
    }

    private async Task<FamilyMember> CreateDefaultOwnerAsync(Guid accountId)
    {
        // 1. Ensure Account exists (to prevent FK violation)
        var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
        if (account == null)
        {
            // If we are using the fallback ID (or any other missing ID), we must create the parent Account first.
            _logger.LogWarning("Account {AccountId} does not exist. creating it for seeding.", accountId);

            // We need a userId - if unavailable, we can use the same GUID or Empty
            var userId = accountId;

            account = new Account
            {
                Id = accountId,
                UserId = userId,
                EncryptedAccountName = "Seeded Account",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EncryptedMasterKey = "",
                MasterKeySalt = "",
                AuthenticationTag = ""
            };

            await _unitOfWork.Accounts.AddAsync(account);
        }

        var owner = new FamilyMember
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            EncryptedRelationship = "U2VsZg==", // Base64 for "Self" (Mock)
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            // Mock encrypted data
            EncryptedFirstName = "Account",
            EncryptedLastName = "Owner",
            InitialsPlainText = "AO",
            AvatarColor = "bg-blue-600"
        };

        await _unitOfWork.FamilyMembers.AddAsync(owner);

        // Save changes (Account + FamilyMember)
        await _unitOfWork.CompleteAsync();

        return owner;
    }

    public async Task<IEnumerable<FamilyMember>> GetFamilyMembersByUserIdAsync(Guid userId)
    {
        // 1. Find Account by UserId
        var account = (await _unitOfWork.Accounts.FindAsync(a => a.UserId == userId))
                      .OrderByDescending(a => a.IsDefault)
                      .FirstOrDefault();

        // 2. If no account, create one (Auto-heal)
        if (account == null)
        {
            _logger.LogInformation("No account found for user {UserId}. Creating new account...", userId);

            // Generate a deterministic or random Account ID
            var accountId = Guid.NewGuid();
            var newAccount = new Account
            {
                Id = accountId,
                UserId = userId,
                EncryptedAccountName = "My Vault",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EncryptedMasterKey = "",
                MasterKeySalt = "",
                AuthenticationTag = ""
            };

            await _unitOfWork.Accounts.AddAsync(newAccount);
            // Save immediately to ensure ID exists for FamilyMember
            await _unitOfWork.CompleteAsync();

            account = newAccount;
        }

        return await GetFamilyMembersAsync(account.Id);
    }

    public async Task<FamilyMember?> GetFamilyMemberByUserIdAsync(Guid id, Guid userId)
    {
        var account = (await _unitOfWork.Accounts.FindAsync(a => a.UserId == userId))
                     .OrderByDescending(a => a.IsDefault)
                     .FirstOrDefault();

        if (account == null) return null;

        return await GetFamilyMemberAsync(id, account.Id);
    }
}
