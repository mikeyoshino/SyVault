using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalVault.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PhoneVerified = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Salt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MfaEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MfaSecret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    KeyDerivationSalt = table.Column<byte[]>(type: "bytea", nullable: false),
                    KeyDerivationIterations = table.Column<int>(type: "integer", nullable: false),
                    SubscriptionTier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Free"),
                    SubscriptionExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldValue = table.Column<string>(type: "jsonb", nullable: true),
                    NewValue = table.Column<string>(type: "jsonb", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DeadManSwitches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckInIntervalDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 90),
                    GracePeriodDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 14),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LastCheckInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextCheckInDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    GracePeriodStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TriggeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReminderDays = table.Column<string>(type: "text", nullable: false),
                    NotificationChannels = table.Column<string>(type: "text", nullable: false),
                    EmergencyEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    EmergencyPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeadManSwitches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeadManSwitches_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Heirs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Relationship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerificationToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VerificationExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublicKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    EncryptedPrivateKey = table.Column<byte[]>(type: "bytea", nullable: true),
                    AccessLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Full"),
                    CanAccessCategories = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Heirs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Heirs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VaultEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EncryptedDataKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    EncryptedContent = table.Column<byte[]>(type: "bytea", nullable: true),
                    BlobStorageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BlobStorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IV = table.Column<byte[]>(type: "bytea", nullable: false),
                    EncryptionAlgorithm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "AES-256-GCM"),
                    IsSharedWithHeirs = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaultEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VaultEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SwitchCheckIns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SwitchId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckInMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwitchCheckIns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SwitchCheckIns_DeadManSwitches_SwitchId",
                        column: x => x.SwitchId,
                        principalTable: "DeadManSwitches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SwitchCheckIns_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SwitchNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SwitchId = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "sent"),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Body = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    CheckInLinkToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CheckInLinkExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WasClickedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwitchNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SwitchNotifications_DeadManSwitches_SwitchId",
                        column: x => x.SwitchId,
                        principalTable: "DeadManSwitches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HeirAccessLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeirId = table.Column<Guid>(type: "uuid", nullable: false),
                    VaultEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WasSuccessful = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeirAccessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeirAccessLogs_Heirs_HeirId",
                        column: x => x.HeirId,
                        principalTable: "Heirs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HeirAccessLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HeirAccessLogs_VaultEntries_VaultEntryId",
                        column: x => x.VaultEntryId,
                        principalTable: "VaultEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HeirVaultAccesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeirId = table.Column<Guid>(type: "uuid", nullable: false),
                    VaultEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncryptedDataKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    CanAccess = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AccessGrantedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeirVaultAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeirVaultAccesses_Heirs_HeirId",
                        column: x => x.HeirId,
                        principalTable: "Heirs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HeirVaultAccesses_VaultEntries_VaultEntryId",
                        column: x => x.VaultEntryId,
                        principalTable: "VaultEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityId",
                table: "AuditLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType",
                table: "AuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeadManSwitches_IsActive",
                table: "DeadManSwitches",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DeadManSwitches_NextCheckInDueDate",
                table: "DeadManSwitches",
                column: "NextCheckInDueDate");

            migrationBuilder.CreateIndex(
                name: "IX_DeadManSwitches_Status",
                table: "DeadManSwitches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DeadManSwitches_UserId",
                table: "DeadManSwitches",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeirAccessLogs_AccessedAt",
                table: "HeirAccessLogs",
                column: "AccessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HeirAccessLogs_AccessType",
                table: "HeirAccessLogs",
                column: "AccessType");

            migrationBuilder.CreateIndex(
                name: "IX_HeirAccessLogs_HeirId",
                table: "HeirAccessLogs",
                column: "HeirId");

            migrationBuilder.CreateIndex(
                name: "IX_HeirAccessLogs_UserId",
                table: "HeirAccessLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeirAccessLogs_VaultEntryId",
                table: "HeirAccessLogs",
                column: "VaultEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Heirs_Email",
                table: "Heirs",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Heirs_IsDeleted",
                table: "Heirs",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Heirs_IsVerified",
                table: "Heirs",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_Heirs_UserId",
                table: "Heirs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Heirs_UserId_Email",
                table: "Heirs",
                columns: new[] { "UserId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeirVaultAccesses_HeirId",
                table: "HeirVaultAccesses",
                column: "HeirId");

            migrationBuilder.CreateIndex(
                name: "IX_HeirVaultAccesses_HeirId_VaultEntryId",
                table: "HeirVaultAccesses",
                columns: new[] { "HeirId", "VaultEntryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeirVaultAccesses_VaultEntryId",
                table: "HeirVaultAccesses",
                column: "VaultEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_SwitchCheckIns_CheckInAt",
                table: "SwitchCheckIns",
                column: "CheckInAt");

            migrationBuilder.CreateIndex(
                name: "IX_SwitchCheckIns_SwitchId",
                table: "SwitchCheckIns",
                column: "SwitchId");

            migrationBuilder.CreateIndex(
                name: "IX_SwitchCheckIns_UserId",
                table: "SwitchCheckIns",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SwitchNotifications_CheckInLinkToken",
                table: "SwitchNotifications",
                column: "CheckInLinkToken",
                unique: true,
                filter: "\"CheckInLinkToken\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SwitchNotifications_NotificationType",
                table: "SwitchNotifications",
                column: "NotificationType");

            migrationBuilder.CreateIndex(
                name: "IX_SwitchNotifications_SentAt",
                table: "SwitchNotifications",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_SwitchNotifications_SwitchId",
                table: "SwitchNotifications",
                column: "SwitchId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsDeleted",
                table: "Users",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SubscriptionTier",
                table: "Users",
                column: "SubscriptionTier");

            migrationBuilder.CreateIndex(
                name: "IX_VaultEntries_Category",
                table: "VaultEntries",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_VaultEntries_CreatedAt",
                table: "VaultEntries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VaultEntries_IsDeleted",
                table: "VaultEntries",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_VaultEntries_UserId",
                table: "VaultEntries",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "HeirAccessLogs");

            migrationBuilder.DropTable(
                name: "HeirVaultAccesses");

            migrationBuilder.DropTable(
                name: "SwitchCheckIns");

            migrationBuilder.DropTable(
                name: "SwitchNotifications");

            migrationBuilder.DropTable(
                name: "Heirs");

            migrationBuilder.DropTable(
                name: "VaultEntries");

            migrationBuilder.DropTable(
                name: "DeadManSwitches");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
