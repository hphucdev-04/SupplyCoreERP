using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AbpAuditLogExcelFiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpAuditLogExcelFiles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpAuditLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ApplicationName = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: true),
                UserId = table.Column<Guid>(type: "uuid", nullable: true),
                UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                TenantName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                ImpersonatorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                ImpersonatorUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                ImpersonatorTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                ImpersonatorTenantName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                ExecutionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                ExecutionDuration = table.Column<int>(type: "integer", nullable: false),
                ClientIpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                ClientName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                ClientId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                CorrelationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                BrowserInfo = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                HttpMethod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                Url = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                Exceptions = table.Column<string>(type: "text", nullable: true),
                Comments = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpAuditLogs", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpBackgroundJobs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ApplicationName = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: true),
                JobName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                JobArgs = table.Column<string>(type: "character varying(1048576)", maxLength: 1048576, nullable: false),
                TryCount = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                NextTryTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                LastTryTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                IsAbandoned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                Priority = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)15),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpBackgroundJobs", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpBlobContainers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpBlobContainers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpClaimTypes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Required = table.Column<bool>(type: "boolean", nullable: false),
                IsStatic = table.Column<bool>(type: "boolean", nullable: false),
                Regex = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                RegexDescription = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                ValueType = table.Column<int>(type: "integer", nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpClaimTypes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpFeatureGroups",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpFeatureGroups", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpFeatures",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                GroupName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ParentName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                DefaultValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                IsVisibleToClients = table.Column<bool>(type: "boolean", nullable: false),
                IsAvailableToHost = table.Column<bool>(type: "boolean", nullable: false),
                AllowedProviders = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                ValueType = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                ExtraProperties = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpFeatures", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpFeatureValues",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ProviderName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpFeatureValues", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpLinkUsers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SourceUserId = table.Column<Guid>(type: "uuid", nullable: false),
                SourceTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                TargetUserId = table.Column<Guid>(type: "uuid", nullable: false),
                TargetTenantId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpLinkUsers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpOrganizationUnits",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                Code = table.Column<string>(type: "character varying(95)", maxLength: 95, nullable: false),
                DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                EntityVersion = table.Column<int>(type: "integer", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpOrganizationUnits", x => x.Id);
                table.ForeignKey(
                    name: "FK_AbpOrganizationUnits_AbpOrganizationUnits_ParentId",
                    column: x => x.ParentId,
                    principalTable: "AbpOrganizationUnits",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "AbpPermissionGrants",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ProviderName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpPermissionGrants", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpPermissionGroups",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpPermissionGroups", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpPermissions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                GroupName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ParentName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                MultiTenancySide = table.Column<byte>(type: "smallint", nullable: false),
                Providers = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                StateCheckers = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                ExtraProperties = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpPermissions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpRoles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                IsStatic = table.Column<bool>(type: "boolean", nullable: false),
                IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                EntityVersion = table.Column<int>(type: "integer", nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpRoles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpSecurityLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                ApplicationName = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: true),
                Identity = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: true),
                Action = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: true),
                UserId = table.Column<Guid>(type: "uuid", nullable: true),
                UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                TenantName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                ClientId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                CorrelationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                ClientIpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                BrowserInfo = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpSecurityLogs", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpSessions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SessionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Device = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                DeviceInfo = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                ClientId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                IpAddresses = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                SignedIn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                LastAccessed = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                ExtraProperties = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpSessions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpSettingDefinitions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                DefaultValue = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                IsVisibleToClients = table.Column<bool>(type: "boolean", nullable: false),
                Providers = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                IsInherited = table.Column<bool>(type: "boolean", nullable: false),
                IsEncrypted = table.Column<bool>(type: "boolean", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpSettingDefinitions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpSettings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Value = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                ProviderName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpSettings", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpUserDelegations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                SourceUserId = table.Column<Guid>(type: "uuid", nullable: false),
                TargetUserId = table.Column<Guid>(type: "uuid", nullable: false),
                StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                EndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpUserDelegations", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpUsers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                Surname = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                SecurityStamp = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                IsExternal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                PhoneNumber = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                AccessFailedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                ShouldChangePasswordOnNextLogin = table.Column<bool>(type: "boolean", nullable: false),
                EntityVersion = table.Column<int>(type: "integer", nullable: false),
                LastPasswordChangeTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpUsers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AppActiveIngredients",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppActiveIngredients", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AppBaseUnits",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppBaseUnits", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AppCategories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppCategories", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AppContinents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppContinents", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AppDocumentSequences",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DocumentType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                PrefixDate = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                LastValue = table.Column<int>(type: "integer", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppDocumentSequences", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AppDosageForms",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppDosageForms", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AppInventoryReservations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ReferenceDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                ReferenceDocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                BinId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                ReservedQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppInventoryReservations", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AppPriceLists",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Currency = table.Column<int>(type: "integer", nullable: false),
                IsBase = table.Column<bool>(type: "boolean", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppPriceLists", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "OpenIddictApplications",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ApplicationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                ClientId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                ClientSecret = table.Column<string>(type: "text", nullable: true),
                ClientType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                ConsentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                DisplayName = table.Column<string>(type: "text", nullable: true),
                DisplayNames = table.Column<string>(type: "text", nullable: true),
                JsonWebKeySet = table.Column<string>(type: "text", nullable: true),
                Permissions = table.Column<string>(type: "text", nullable: true),
                PostLogoutRedirectUris = table.Column<string>(type: "text", nullable: true),
                Properties = table.Column<string>(type: "text", nullable: true),
                RedirectUris = table.Column<string>(type: "text", nullable: true),
                Requirements = table.Column<string>(type: "text", nullable: true),
                Settings = table.Column<string>(type: "text", nullable: true),
                FrontChannelLogoutUri = table.Column<string>(type: "text", nullable: true),
                ClientUri = table.Column<string>(type: "text", nullable: true),
                LogoUri = table.Column<string>(type: "text", nullable: true),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OpenIddictApplications", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "OpenIddictScopes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Description = table.Column<string>(type: "text", nullable: true),
                Descriptions = table.Column<string>(type: "text", nullable: true),
                DisplayName = table.Column<string>(type: "text", nullable: true),
                DisplayNames = table.Column<string>(type: "text", nullable: true),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                Properties = table.Column<string>(type: "text", nullable: true),
                Resources = table.Column<string>(type: "text", nullable: true),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OpenIddictScopes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AbpAuditLogActions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                AuditLogId = table.Column<Guid>(type: "uuid", nullable: false),
                ServiceName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                MethodName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                Parameters = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                ExecutionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                ExecutionDuration = table.Column<int>(type: "integer", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpAuditLogActions", x => x.Id);
                table.ForeignKey(
                    name: "FK_AbpAuditLogActions_AbpAuditLogs_AuditLogId",
                    column: x => x.AuditLogId,
                    principalTable: "AbpAuditLogs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AbpEntityChanges",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                AuditLogId = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                ChangeTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                ChangeType = table.Column<byte>(type: "smallint", nullable: false),
                EntityTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                EntityTypeFullName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpEntityChanges", x => x.Id);
                table.ForeignKey(
                    name: "FK_AbpEntityChanges_AbpAuditLogs_AuditLogId",
                    column: x => x.AuditLogId,
                    principalTable: "AbpAuditLogs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AbpBlobs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ContainerId = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Content = table.Column<byte[]>(type: "bytea", maxLength: 2147483647, nullable: true),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpBlobs", x => x.Id);
                table.ForeignKey(
                    name: "FK_AbpBlobs_AbpBlobContainers_ContainerId",
                    column: x => x.ContainerId,
                    principalTable: "AbpBlobContainers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AbpOrganizationUnitRoles",
            columns: table => new
            {
                RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpOrganizationUnitRoles", x => new { x.OrganizationUnitId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AbpOrganizationUnitRoles_AbpOrganizationUnits_OrganizationU~",
                    column: x => x.OrganizationUnitId,
                    principalTable: "AbpOrganizationUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AbpOrganizationUnitRoles_AbpRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AbpRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AbpRoleClaims",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                ClaimType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                ClaimValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AbpRoleClaims_AbpRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AbpRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AbpUserClaims",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                ClaimType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                ClaimValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AbpUserClaims_AbpUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AbpUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AbpUserLogins",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                LoginProvider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                ProviderKey = table.Column<string>(type: "character varying(196)", maxLength: 196, nullable: false),
                ProviderDisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpUserLogins", x => new { x.UserId, x.LoginProvider });
                table.ForeignKey(
                    name: "FK_AbpUserLogins_AbpUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AbpUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AbpUserOrganizationUnits",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpUserOrganizationUnits", x => new { x.OrganizationUnitId, x.UserId });
                table.ForeignKey(
                    name: "FK_AbpUserOrganizationUnits_AbpOrganizationUnits_OrganizationU~",
                    column: x => x.OrganizationUnitId,
                    principalTable: "AbpOrganizationUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AbpUserOrganizationUnits_AbpUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AbpUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AbpUserRoles",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AbpUserRoles_AbpRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AbpRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AbpUserRoles_AbpUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AbpUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AbpUserTokens",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                LoginProvider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                Value = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AbpUserTokens_AbpUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AbpUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AppCountries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ContinentId = table.Column<Guid>(type: "uuid", nullable: false),
                ISO = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppCountries", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppCountries_AppContinents_ContinentId",
                    column: x => x.ContinentId,
                    principalTable: "AppContinents",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "OpenIddictAuthorizations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                Properties = table.Column<string>(type: "text", nullable: true),
                Scopes = table.Column<string>(type: "text", nullable: true),
                Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OpenIddictAuthorizations", x => x.Id);
                table.ForeignKey(
                    name: "FK_OpenIddictAuthorizations_OpenIddictApplications_Application~",
                    column: x => x.ApplicationId,
                    principalTable: "OpenIddictApplications",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "AbpEntityPropertyChanges",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                EntityChangeId = table.Column<Guid>(type: "uuid", nullable: false),
                NewValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                OriginalValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                PropertyName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                PropertyTypeFullName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbpEntityPropertyChanges", x => x.Id);
                table.ForeignKey(
                    name: "FK_AbpEntityPropertyChanges_AbpEntityChanges_EntityChangeId",
                    column: x => x.EntityChangeId,
                    principalTable: "AbpEntityChanges",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AppCities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppCities", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppCities_AppCountries_CountryId",
                    column: x => x.CountryId,
                    principalTable: "AppCountries",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AppManufacturers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                ContinentId = table.Column<Guid>(type: "uuid", nullable: false),
                CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppManufacturers", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppManufacturers_AppContinents_ContinentId",
                    column: x => x.ContinentId,
                    principalTable: "AppContinents",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppManufacturers_AppCountries_CountryId",
                    column: x => x.CountryId,
                    principalTable: "AppCountries",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "OpenIddictTokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                AuthorizationId = table.Column<Guid>(type: "uuid", nullable: true),
                CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                ExpirationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                Payload = table.Column<string>(type: "text", nullable: true),
                Properties = table.Column<string>(type: "text", nullable: true),
                RedemptionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                ReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                Type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OpenIddictTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_OpenIddictTokens_OpenIddictApplications_ApplicationId",
                    column: x => x.ApplicationId,
                    principalTable: "OpenIddictApplications",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId",
                    column: x => x.AuthorizationId,
                    principalTable: "OpenIddictAuthorizations",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "AppAreas",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CityId = table.Column<Guid>(type: "uuid", nullable: false),
                ZipCode = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppAreas", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppAreas_AppCities_CityId",
                    column: x => x.CityId,
                    principalTable: "AppCities",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AppProducts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                ManufacturerId = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                BaseUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductType = table.Column<int>(type: "integer", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppProducts", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppProducts_AppBaseUnits_BaseUnitId",
                    column: x => x.BaseUnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppProducts_AppCategories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "AppCategories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppProducts_AppManufacturers_ManufacturerId",
                    column: x => x.ManufacturerId,
                    principalTable: "AppManufacturers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AppCustomers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                PhoneNumber = table.Column<string>(type: "text", nullable: true),
                Email = table.Column<string>(type: "text", nullable: true),
                RepresentativeName = table.Column<string>(type: "text", nullable: true),
                Gender = table.Column<int>(type: "integer", nullable: true),
                Type = table.Column<int>(type: "integer", nullable: false),
                TaxCode = table.Column<string>(type: "text", nullable: true),
                Note = table.Column<string>(type: "text", nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                Address = table.Column<string>(type: "text", nullable: true),
                CountryId = table.Column<Guid>(type: "uuid", nullable: true),
                CityId = table.Column<Guid>(type: "uuid", nullable: true),
                AreaId = table.Column<Guid>(type: "uuid", nullable: true),
                DebtLimit = table.Column<decimal>(type: "numeric", nullable: false),
                PaymentTermDays = table.Column<int>(type: "integer", nullable: false),
                CurrentDebt = table.Column<decimal>(type: "numeric", nullable: false),
                PriceListId = table.Column<Guid>(type: "uuid", nullable: true),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppCustomers", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppCustomers_AppAreas_AreaId",
                    column: x => x.AreaId,
                    principalTable: "AppAreas",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_AppCustomers_AppCities_CityId",
                    column: x => x.CityId,
                    principalTable: "AppCities",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_AppCustomers_AppCountries_CountryId",
                    column: x => x.CountryId,
                    principalTable: "AppCountries",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_AppCustomers_AppPriceLists_PriceListId",
                    column: x => x.PriceListId,
                    principalTable: "AppPriceLists",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "AppSuppliers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                TaxCode = table.Column<string>(type: "text", nullable: true),
                PhoneNumber = table.Column<string>(type: "text", nullable: true),
                Email = table.Column<string>(type: "text", nullable: true),
                RepresentativeName = table.Column<string>(type: "text", nullable: true),
                Gender = table.Column<int>(type: "integer", nullable: true),
                Note = table.Column<string>(type: "text", nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                Address = table.Column<string>(type: "text", nullable: true),
                DebtLimit = table.Column<decimal>(type: "numeric", nullable: false),
                PaymentTermDays = table.Column<int>(type: "integer", nullable: false),
                CurrentDebt = table.Column<decimal>(type: "numeric", nullable: false),
                CountryId = table.Column<Guid>(type: "uuid", nullable: true),
                CityId = table.Column<Guid>(type: "uuid", nullable: true),
                AreaId = table.Column<Guid>(type: "uuid", nullable: true),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppSuppliers", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppSuppliers_AppAreas_AreaId",
                    column: x => x.AreaId,
                    principalTable: "AppAreas",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_AppSuppliers_AppCities_CityId",
                    column: x => x.CityId,
                    principalTable: "AppCities",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_AppSuppliers_AppCountries_CountryId",
                    column: x => x.CountryId,
                    principalTable: "AppCountries",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "AppWarehouses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CityId = table.Column<Guid>(type: "uuid", nullable: true),
                AreaId = table.Column<Guid>(type: "uuid", nullable: true),
                MapWidth = table.Column<int>(type: "integer", nullable: false),
                MapLength = table.Column<int>(type: "integer", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppWarehouses", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppWarehouses_AppAreas_AreaId",
                    column: x => x.AreaId,
                    principalTable: "AppAreas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppWarehouses_AppCities_CityId",
                    column: x => x.CityId,
                    principalTable: "AppCities",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AppMedicines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DosageFormId = table.Column<Guid>(type: "uuid", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                RegistrationNumber = table.Column<string>(type: "text", nullable: false),
                UsageRoute = table.Column<int>(type: "integer", nullable: false),
                StorageCondition = table.Column<int>(type: "integer", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                IsPrescriptionDrug = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppMedicines", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppMedicines_AppDosageForms_DosageFormId",
                    column: x => x.DosageFormId,
                    principalTable: "AppDosageForms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppMedicines_AppProducts_Id",
                    column: x => x.Id,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AppProductPrices",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PriceListId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                MinQuantity = table.Column<int>(type: "integer", nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppProductPrices", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppProductPrices_AppBaseUnits_UnitId",
                    column: x => x.UnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppProductPrices_AppPriceLists_PriceListId",
                    column: x => x.PriceListId,
                    principalTable: "AppPriceLists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AppProductPrices_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AppProductUnits",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                ConversionFactor = table.Column<int>(type: "integer", nullable: false),
                Level = table.Column<int>(type: "integer", nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppProductUnits", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppProductUnits_AppBaseUnits_UnitId",
                    column: x => x.UnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppProductUnits_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AppProductBatches",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                ManufacturingDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                ExpiryDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppProductBatches", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppProductBatches_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppProductBatches_AppSuppliers_SupplierId",
                    column: x => x.SupplierId,
                    principalTable: "AppSuppliers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AppInventoryTickets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TicketNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                ReferenceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                ReferenceDocumentNumber = table.Column<string>(type: "text", nullable: true),
                Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppInventoryTickets", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppInventoryTickets_AppWarehouses_WarehouseId",
                    column: x => x.WarehouseId,
                    principalTable: "AppWarehouses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AppPurchaseOrders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                OrderDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                ExpectedDeliveryDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                DueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                SubTotal = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                TaxAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                TotalAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppPurchaseOrders", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppPurchaseOrders_AppSuppliers_SupplierId",
                    column: x => x.SupplierId,
                    principalTable: "AppSuppliers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AppPurchaseOrders_AppWarehouses_WarehouseId",
                    column: x => x.WarehouseId,
                    principalTable: "AppWarehouses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AppSalesOrders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                OrderDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                ExpectedDeliveryDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                DueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                SubTotal = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                DiscountAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                TaxAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                TotalAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppSalesOrders", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppSalesOrders_AppCustomers_CustomerId",
                    column: x => x.CustomerId,
                    principalTable: "AppCustomers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppSalesOrders_AppWarehouses_WarehouseId",
                    column: x => x.WarehouseId,
                    principalTable: "AppWarehouses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AppZones",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                StorageCondition = table.Column<int>(type: "integer", nullable: false),
                Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                PositionX = table.Column<int>(type: "integer", nullable: false),
                PositionY = table.Column<int>(type: "integer", nullable: false),
                Width = table.Column<int>(type: "integer", nullable: false),
                Length = table.Column<int>(type: "integer", nullable: false),
                Rotation = table.Column<float>(type: "real", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppZones", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppZones_AppWarehouses_WarehouseId",
                    column: x => x.WarehouseId,
                    principalTable: "AppWarehouses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AppMedicineIngredients",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MedicineId = table.Column<Guid>(type: "uuid", nullable: false),
                ActiveIngredientId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppMedicineIngredients", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppMedicineIngredients_AppActiveIngredients_ActiveIngredien~",
                    column: x => x.ActiveIngredientId,
                    principalTable: "AppActiveIngredients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppMedicineIngredients_AppMedicines_MedicineId",
                    column: x => x.MedicineId,
                    principalTable: "AppMedicines",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AppPurchaseOrderDetails",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                ConversionFactor = table.Column<int>(type: "integer", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                ReceivedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppPurchaseOrderDetails", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppPurchaseOrderDetails_AppBaseUnits_UnitId",
                    column: x => x.UnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseOrderDetails_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppPurchaseOrderDetails_AppPurchaseOrders_PurchaseOrderId",
                    column: x => x.PurchaseOrderId,
                    principalTable: "AppPurchaseOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AppSalesOrderDetails",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SalesOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                ConversionFactor = table.Column<int>(type: "integer", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                DeliveredQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                DiscountRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppSalesOrderDetails", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppSalesOrderDetails_AppBaseUnits_UnitId",
                    column: x => x.UnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppSalesOrderDetails_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppSalesOrderDetails_AppSalesOrders_SalesOrderId",
                    column: x => x.SalesOrderId,
                    principalTable: "AppSalesOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AppBins",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                ZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                PositionX = table.Column<int>(type: "integer", nullable: false),
                PositionY = table.Column<int>(type: "integer", nullable: false),
                Width = table.Column<int>(type: "integer", nullable: false),
                Length = table.Column<int>(type: "integer", nullable: false),
                Rotation = table.Column<float>(type: "real", nullable: false),
                MaxSKU = table.Column<int>(type: "integer", nullable: false),
                IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppBins", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppBins_AppWarehouses_WarehouseId",
                    column: x => x.WarehouseId,
                    principalTable: "AppWarehouses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AppBins_AppZones_ZoneId",
                    column: x => x.ZoneId,
                    principalTable: "AppZones",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AppInventoryBalances",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                BinId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                LockedQuantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppInventoryBalances", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppInventoryBalances_AppBins_BinId",
                    column: x => x.BinId,
                    principalTable: "AppBins",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppInventoryBalances_AppProductBatches_ProductBatchId",
                    column: x => x.ProductBatchId,
                    principalTable: "AppProductBatches",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppInventoryBalances_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppInventoryBalances_AppWarehouses_WarehouseId",
                    column: x => x.WarehouseId,
                    principalTable: "AppWarehouses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AppInventoryTicketDetails",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                BinId = table.Column<Guid>(type: "uuid", nullable: false),
                UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                ConversionFactor = table.Column<int>(type: "integer", nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppInventoryTicketDetails", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppInventoryTicketDetails_AppBaseUnits_UnitId",
                    column: x => x.UnitId,
                    principalTable: "AppBaseUnits",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_AppInventoryTicketDetails_AppBins_BinId",
                    column: x => x.BinId,
                    principalTable: "AppBins",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppInventoryTicketDetails_AppInventoryTickets_TicketId",
                    column: x => x.TicketId,
                    principalTable: "AppInventoryTickets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AppInventoryTicketDetails_AppProductBatches_ProductBatchId",
                    column: x => x.ProductBatchId,
                    principalTable: "AppProductBatches",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppInventoryTicketDetails_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "AppInventoryTransactions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                BinId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                TransactionType = table.Column<int>(type: "integer", nullable: false),
                QuantityChanged = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                BalanceAfterTransaction = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                ReferenceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                ReferenceDocumentNumber = table.Column<string>(type: "text", nullable: true),
                Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                ExtraProperties = table.Column<string>(type: "text", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppInventoryTransactions", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppInventoryTransactions_AppBins_BinId",
                    column: x => x.BinId,
                    principalTable: "AppBins",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppInventoryTransactions_AppProductBatches_ProductBatchId",
                    column: x => x.ProductBatchId,
                    principalTable: "AppProductBatches",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppInventoryTransactions_AppProducts_ProductId",
                    column: x => x.ProductId,
                    principalTable: "AppProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AppInventoryTransactions_AppWarehouses_WarehouseId",
                    column: x => x.WarehouseId,
                    principalTable: "AppWarehouses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AbpAuditLogActions_AuditLogId",
            table: "AbpAuditLogActions",
            column: "AuditLogId");

        migrationBuilder.CreateIndex(
            name: "IX_AbpAuditLogActions_TenantId_ServiceName_MethodName_Executio~",
            table: "AbpAuditLogActions",
            columns: new[] { "TenantId", "ServiceName", "MethodName", "ExecutionTime" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpAuditLogs_TenantId_ExecutionTime",
            table: "AbpAuditLogs",
            columns: new[] { "TenantId", "ExecutionTime" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpAuditLogs_TenantId_UserId_ExecutionTime",
            table: "AbpAuditLogs",
            columns: new[] { "TenantId", "UserId", "ExecutionTime" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpBackgroundJobs_IsAbandoned_NextTryTime",
            table: "AbpBackgroundJobs",
            columns: new[] { "IsAbandoned", "NextTryTime" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpBlobContainers_TenantId_Name",
            table: "AbpBlobContainers",
            columns: new[] { "TenantId", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpBlobs_ContainerId",
            table: "AbpBlobs",
            column: "ContainerId");

        migrationBuilder.CreateIndex(
            name: "IX_AbpBlobs_TenantId_ContainerId_Name",
            table: "AbpBlobs",
            columns: new[] { "TenantId", "ContainerId", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpEntityChanges_AuditLogId",
            table: "AbpEntityChanges",
            column: "AuditLogId");

        migrationBuilder.CreateIndex(
            name: "IX_AbpEntityChanges_TenantId_EntityTypeFullName_EntityId",
            table: "AbpEntityChanges",
            columns: new[] { "TenantId", "EntityTypeFullName", "EntityId" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpEntityPropertyChanges_EntityChangeId",
            table: "AbpEntityPropertyChanges",
            column: "EntityChangeId");

        migrationBuilder.CreateIndex(
            name: "IX_AbpFeatureGroups_Name",
            table: "AbpFeatureGroups",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AbpFeatures_GroupName",
            table: "AbpFeatures",
            column: "GroupName");

        migrationBuilder.CreateIndex(
            name: "IX_AbpFeatures_Name",
            table: "AbpFeatures",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AbpFeatureValues_Name_ProviderName_ProviderKey",
            table: "AbpFeatureValues",
            columns: new[] { "Name", "ProviderName", "ProviderKey" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AbpLinkUsers_SourceUserId_SourceTenantId_TargetUserId_Targe~",
            table: "AbpLinkUsers",
            columns: new[] { "SourceUserId", "SourceTenantId", "TargetUserId", "TargetTenantId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AbpOrganizationUnitRoles_RoleId_OrganizationUnitId",
            table: "AbpOrganizationUnitRoles",
            columns: new[] { "RoleId", "OrganizationUnitId" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpOrganizationUnits_Code",
            table: "AbpOrganizationUnits",
            column: "Code");

        migrationBuilder.CreateIndex(
            name: "IX_AbpOrganizationUnits_ParentId",
            table: "AbpOrganizationUnits",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_AbpPermissionGrants_TenantId_Name_ProviderName_ProviderKey",
            table: "AbpPermissionGrants",
            columns: new[] { "TenantId", "Name", "ProviderName", "ProviderKey" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AbpPermissionGroups_Name",
            table: "AbpPermissionGroups",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AbpPermissions_GroupName",
            table: "AbpPermissions",
            column: "GroupName");

        migrationBuilder.CreateIndex(
            name: "IX_AbpPermissions_Name",
            table: "AbpPermissions",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AbpRoleClaims_RoleId",
            table: "AbpRoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "IX_AbpRoles_NormalizedName",
            table: "AbpRoles",
            column: "NormalizedName");

        migrationBuilder.CreateIndex(
            name: "IX_AbpSecurityLogs_TenantId_Action",
            table: "AbpSecurityLogs",
            columns: new[] { "TenantId", "Action" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpSecurityLogs_TenantId_ApplicationName",
            table: "AbpSecurityLogs",
            columns: new[] { "TenantId", "ApplicationName" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpSecurityLogs_TenantId_Identity",
            table: "AbpSecurityLogs",
            columns: new[] { "TenantId", "Identity" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpSecurityLogs_TenantId_UserId",
            table: "AbpSecurityLogs",
            columns: new[] { "TenantId", "UserId" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpSessions_Device",
            table: "AbpSessions",
            column: "Device");

        migrationBuilder.CreateIndex(
            name: "IX_AbpSessions_SessionId",
            table: "AbpSessions",
            column: "SessionId");

        migrationBuilder.CreateIndex(
            name: "IX_AbpSessions_TenantId_UserId",
            table: "AbpSessions",
            columns: new[] { "TenantId", "UserId" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpSettingDefinitions_Name",
            table: "AbpSettingDefinitions",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AbpSettings_Name_ProviderName_ProviderKey",
            table: "AbpSettings",
            columns: new[] { "Name", "ProviderName", "ProviderKey" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AbpUserClaims_UserId",
            table: "AbpUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AbpUserLogins_LoginProvider_ProviderKey",
            table: "AbpUserLogins",
            columns: new[] { "LoginProvider", "ProviderKey" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpUserOrganizationUnits_UserId_OrganizationUnitId",
            table: "AbpUserOrganizationUnits",
            columns: new[] { "UserId", "OrganizationUnitId" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpUserRoles_RoleId_UserId",
            table: "AbpUserRoles",
            columns: new[] { "RoleId", "UserId" });

        migrationBuilder.CreateIndex(
            name: "IX_AbpUsers_Email",
            table: "AbpUsers",
            column: "Email");

        migrationBuilder.CreateIndex(
            name: "IX_AbpUsers_NormalizedEmail",
            table: "AbpUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "IX_AbpUsers_NormalizedUserName",
            table: "AbpUsers",
            column: "NormalizedUserName");

        migrationBuilder.CreateIndex(
            name: "IX_AbpUsers_UserName",
            table: "AbpUsers",
            column: "UserName");

        migrationBuilder.CreateIndex(
            name: "IX_AppActiveIngredients_Code",
            table: "AppActiveIngredients",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppAreas_CityId",
            table: "AppAreas",
            column: "CityId");

        migrationBuilder.CreateIndex(
            name: "IX_AppBaseUnits_Code",
            table: "AppBaseUnits",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppBins_WarehouseId_Code",
            table: "AppBins",
            columns: new[] { "WarehouseId", "Code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppBins_ZoneId",
            table: "AppBins",
            column: "ZoneId");

        migrationBuilder.CreateIndex(
            name: "IX_AppCategories_Name",
            table: "AppCategories",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppCities_CountryId",
            table: "AppCities",
            column: "CountryId");

        migrationBuilder.CreateIndex(
            name: "IX_AppCountries_ContinentId",
            table: "AppCountries",
            column: "ContinentId");

        migrationBuilder.CreateIndex(
            name: "IX_AppCountries_ISO",
            table: "AppCountries",
            column: "ISO",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppCustomers_AreaId",
            table: "AppCustomers",
            column: "AreaId");

        migrationBuilder.CreateIndex(
            name: "IX_AppCustomers_CityId",
            table: "AppCustomers",
            column: "CityId");

        migrationBuilder.CreateIndex(
            name: "IX_AppCustomers_Code",
            table: "AppCustomers",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppCustomers_CountryId",
            table: "AppCustomers",
            column: "CountryId");

        migrationBuilder.CreateIndex(
            name: "IX_AppCustomers_PhoneNumber",
            table: "AppCustomers",
            column: "PhoneNumber",
            unique: true,
            filter: "\"PhoneNumber\" IS NOT NULL AND \"PhoneNumber\" != ''");

        migrationBuilder.CreateIndex(
            name: "IX_AppCustomers_PriceListId",
            table: "AppCustomers",
            column: "PriceListId");

        migrationBuilder.CreateIndex(
            name: "IX_AppDocumentSequences_DocumentType",
            table: "AppDocumentSequences",
            column: "DocumentType",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppDosageForms_Code",
            table: "AppDosageForms",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryBalances_BinId",
            table: "AppInventoryBalances",
            column: "BinId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryBalances_ProductBatchId",
            table: "AppInventoryBalances",
            column: "ProductBatchId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryBalances_ProductId",
            table: "AppInventoryBalances",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryBalances_WarehouseId_BinId_ProductId_ProductBat~",
            table: "AppInventoryBalances",
            columns: new[] { "WarehouseId", "BinId", "ProductId", "ProductBatchId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryReservations_ReferenceDocumentId_Status",
            table: "AppInventoryReservations",
            columns: new[] { "ReferenceDocumentId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTicketDetails_BinId",
            table: "AppInventoryTicketDetails",
            column: "BinId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTicketDetails_ProductBatchId",
            table: "AppInventoryTicketDetails",
            column: "ProductBatchId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTicketDetails_ProductId",
            table: "AppInventoryTicketDetails",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTicketDetails_TicketId",
            table: "AppInventoryTicketDetails",
            column: "TicketId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTicketDetails_UnitId",
            table: "AppInventoryTicketDetails",
            column: "UnitId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTickets_ReferenceDocumentId",
            table: "AppInventoryTickets",
            column: "ReferenceDocumentId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTickets_TicketNumber",
            table: "AppInventoryTickets",
            column: "TicketNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTickets_WarehouseId",
            table: "AppInventoryTickets",
            column: "WarehouseId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTransactions_BinId",
            table: "AppInventoryTransactions",
            column: "BinId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTransactions_ProductBatchId",
            table: "AppInventoryTransactions",
            column: "ProductBatchId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTransactions_ProductId",
            table: "AppInventoryTransactions",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTransactions_WarehouseId_ProductId_CreationTime",
            table: "AppInventoryTransactions",
            columns: new[] { "WarehouseId", "ProductId", "CreationTime" });

        migrationBuilder.CreateIndex(
            name: "IX_AppManufacturers_ContinentId",
            table: "AppManufacturers",
            column: "ContinentId");

        migrationBuilder.CreateIndex(
            name: "IX_AppManufacturers_CountryId",
            table: "AppManufacturers",
            column: "CountryId");

        migrationBuilder.CreateIndex(
            name: "IX_AppMedicineIngredients_ActiveIngredientId",
            table: "AppMedicineIngredients",
            column: "ActiveIngredientId");

        migrationBuilder.CreateIndex(
            name: "IX_AppMedicineIngredients_MedicineId",
            table: "AppMedicineIngredients",
            column: "MedicineId");

        migrationBuilder.CreateIndex(
            name: "IX_AppMedicines_DosageFormId",
            table: "AppMedicines",
            column: "DosageFormId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPriceLists_Code",
            table: "AppPriceLists",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppPriceLists_IsBase",
            table: "AppPriceLists",
            column: "IsBase");

        migrationBuilder.CreateIndex(
            name: "IX_AppProductBatches_ProductId_Status_ExpiryDate",
            table: "AppProductBatches",
            columns: new[] { "ProductId", "Status", "ExpiryDate" });

        migrationBuilder.CreateIndex(
            name: "IX_AppProductBatches_SupplierId",
            table: "AppProductBatches",
            column: "SupplierId");

        migrationBuilder.CreateIndex(
            name: "IX_AppProductPrices_PriceListId_ProductId_UnitId_MinQuantity",
            table: "AppProductPrices",
            columns: new[] { "PriceListId", "ProductId", "UnitId", "MinQuantity" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppProductPrices_ProductId",
            table: "AppProductPrices",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppProductPrices_UnitId",
            table: "AppProductPrices",
            column: "UnitId");

        migrationBuilder.CreateIndex(
            name: "IX_AppProducts_BaseUnitId",
            table: "AppProducts",
            column: "BaseUnitId");

        migrationBuilder.CreateIndex(
            name: "IX_AppProducts_CategoryId",
            table: "AppProducts",
            column: "CategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_AppProducts_Code",
            table: "AppProducts",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppProducts_ManufacturerId",
            table: "AppProducts",
            column: "ManufacturerId");

        migrationBuilder.CreateIndex(
            name: "IX_AppProductUnits_ProductId",
            table: "AppProductUnits",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppProductUnits_UnitId",
            table: "AppProductUnits",
            column: "UnitId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseOrderDetails_ProductId",
            table: "AppPurchaseOrderDetails",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseOrderDetails_PurchaseOrderId",
            table: "AppPurchaseOrderDetails",
            column: "PurchaseOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseOrderDetails_UnitId",
            table: "AppPurchaseOrderDetails",
            column: "UnitId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseOrders_SupplierId",
            table: "AppPurchaseOrders",
            column: "SupplierId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPurchaseOrders_WarehouseId",
            table: "AppPurchaseOrders",
            column: "WarehouseId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesOrderDetails_ProductId",
            table: "AppSalesOrderDetails",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesOrderDetails_SalesOrderId",
            table: "AppSalesOrderDetails",
            column: "SalesOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesOrderDetails_UnitId",
            table: "AppSalesOrderDetails",
            column: "UnitId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesOrders_Code",
            table: "AppSalesOrders",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesOrders_CustomerId",
            table: "AppSalesOrders",
            column: "CustomerId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSalesOrders_WarehouseId",
            table: "AppSalesOrders",
            column: "WarehouseId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSuppliers_AreaId",
            table: "AppSuppliers",
            column: "AreaId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSuppliers_CityId",
            table: "AppSuppliers",
            column: "CityId");

        migrationBuilder.CreateIndex(
            name: "IX_AppSuppliers_Code",
            table: "AppSuppliers",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppSuppliers_CountryId",
            table: "AppSuppliers",
            column: "CountryId");

        migrationBuilder.CreateIndex(
            name: "IX_AppWarehouses_AreaId",
            table: "AppWarehouses",
            column: "AreaId");

        migrationBuilder.CreateIndex(
            name: "IX_AppWarehouses_CityId",
            table: "AppWarehouses",
            column: "CityId");

        migrationBuilder.CreateIndex(
            name: "IX_AppWarehouses_Code",
            table: "AppWarehouses",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppZones_WarehouseId_Code",
            table: "AppZones",
            columns: new[] { "WarehouseId", "Code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_OpenIddictApplications_ClientId",
            table: "OpenIddictApplications",
            column: "ClientId");

        migrationBuilder.CreateIndex(
            name: "IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type",
            table: "OpenIddictAuthorizations",
            columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_OpenIddictScopes_Name",
            table: "OpenIddictScopes",
            column: "Name");

        migrationBuilder.CreateIndex(
            name: "IX_OpenIddictTokens_ApplicationId_Status_Subject_Type",
            table: "OpenIddictTokens",
            columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_OpenIddictTokens_AuthorizationId",
            table: "OpenIddictTokens",
            column: "AuthorizationId");

        migrationBuilder.CreateIndex(
            name: "IX_OpenIddictTokens_ReferenceId",
            table: "OpenIddictTokens",
            column: "ReferenceId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AbpAuditLogActions");

        migrationBuilder.DropTable(
            name: "AbpAuditLogExcelFiles");

        migrationBuilder.DropTable(
            name: "AbpBackgroundJobs");

        migrationBuilder.DropTable(
            name: "AbpBlobs");

        migrationBuilder.DropTable(
            name: "AbpClaimTypes");

        migrationBuilder.DropTable(
            name: "AbpEntityPropertyChanges");

        migrationBuilder.DropTable(
            name: "AbpFeatureGroups");

        migrationBuilder.DropTable(
            name: "AbpFeatures");

        migrationBuilder.DropTable(
            name: "AbpFeatureValues");

        migrationBuilder.DropTable(
            name: "AbpLinkUsers");

        migrationBuilder.DropTable(
            name: "AbpOrganizationUnitRoles");

        migrationBuilder.DropTable(
            name: "AbpPermissionGrants");

        migrationBuilder.DropTable(
            name: "AbpPermissionGroups");

        migrationBuilder.DropTable(
            name: "AbpPermissions");

        migrationBuilder.DropTable(
            name: "AbpRoleClaims");

        migrationBuilder.DropTable(
            name: "AbpSecurityLogs");

        migrationBuilder.DropTable(
            name: "AbpSessions");

        migrationBuilder.DropTable(
            name: "AbpSettingDefinitions");

        migrationBuilder.DropTable(
            name: "AbpSettings");

        migrationBuilder.DropTable(
            name: "AbpUserClaims");

        migrationBuilder.DropTable(
            name: "AbpUserDelegations");

        migrationBuilder.DropTable(
            name: "AbpUserLogins");

        migrationBuilder.DropTable(
            name: "AbpUserOrganizationUnits");

        migrationBuilder.DropTable(
            name: "AbpUserRoles");

        migrationBuilder.DropTable(
            name: "AbpUserTokens");

        migrationBuilder.DropTable(
            name: "AppDocumentSequences");

        migrationBuilder.DropTable(
            name: "AppInventoryBalances");

        migrationBuilder.DropTable(
            name: "AppInventoryReservations");

        migrationBuilder.DropTable(
            name: "AppInventoryTicketDetails");

        migrationBuilder.DropTable(
            name: "AppInventoryTransactions");

        migrationBuilder.DropTable(
            name: "AppMedicineIngredients");

        migrationBuilder.DropTable(
            name: "AppProductPrices");

        migrationBuilder.DropTable(
            name: "AppProductUnits");

        migrationBuilder.DropTable(
            name: "AppPurchaseOrderDetails");

        migrationBuilder.DropTable(
            name: "AppSalesOrderDetails");

        migrationBuilder.DropTable(
            name: "OpenIddictScopes");

        migrationBuilder.DropTable(
            name: "OpenIddictTokens");

        migrationBuilder.DropTable(
            name: "AbpBlobContainers");

        migrationBuilder.DropTable(
            name: "AbpEntityChanges");

        migrationBuilder.DropTable(
            name: "AbpOrganizationUnits");

        migrationBuilder.DropTable(
            name: "AbpRoles");

        migrationBuilder.DropTable(
            name: "AbpUsers");

        migrationBuilder.DropTable(
            name: "AppInventoryTickets");

        migrationBuilder.DropTable(
            name: "AppBins");

        migrationBuilder.DropTable(
            name: "AppProductBatches");

        migrationBuilder.DropTable(
            name: "AppActiveIngredients");

        migrationBuilder.DropTable(
            name: "AppMedicines");

        migrationBuilder.DropTable(
            name: "AppPurchaseOrders");

        migrationBuilder.DropTable(
            name: "AppSalesOrders");

        migrationBuilder.DropTable(
            name: "OpenIddictAuthorizations");

        migrationBuilder.DropTable(
            name: "AbpAuditLogs");

        migrationBuilder.DropTable(
            name: "AppZones");

        migrationBuilder.DropTable(
            name: "AppDosageForms");

        migrationBuilder.DropTable(
            name: "AppProducts");

        migrationBuilder.DropTable(
            name: "AppSuppliers");

        migrationBuilder.DropTable(
            name: "AppCustomers");

        migrationBuilder.DropTable(
            name: "OpenIddictApplications");

        migrationBuilder.DropTable(
            name: "AppWarehouses");

        migrationBuilder.DropTable(
            name: "AppBaseUnits");

        migrationBuilder.DropTable(
            name: "AppCategories");

        migrationBuilder.DropTable(
            name: "AppManufacturers");

        migrationBuilder.DropTable(
            name: "AppPriceLists");

        migrationBuilder.DropTable(
            name: "AppAreas");

        migrationBuilder.DropTable(
            name: "AppCities");

        migrationBuilder.DropTable(
            name: "AppCountries");

        migrationBuilder.DropTable(
            name: "AppContinents");
    }
}
