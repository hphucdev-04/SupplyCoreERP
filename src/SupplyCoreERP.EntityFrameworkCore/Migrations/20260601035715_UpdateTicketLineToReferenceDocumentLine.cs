using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyCoreERP.Migrations;

/// <inheritdoc />
public partial class UpdateTicketLineToReferenceDocumentLine : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DO $$
            DECLARE
                r RECORD;
            BEGIN
                FOR r IN 
                    SELECT constraint_name 
                    FROM information_schema.table_constraints 
                    WHERE table_name ILIKE 'AppInventoryTicketLines' 
                      AND constraint_type = 'FOREIGN KEY'
                      AND constraint_name ILIKE 'FK_AppInventoryTicketLines_AppPurchaseOrderLines%'
                LOOP
                    EXECUTE 'ALTER TABLE ""AppInventoryTicketLines"" DROP CONSTRAINT ""' || r.constraint_name || '""';
                END LOOP;
            END $$;");

        migrationBuilder.Sql(@"
            DO $$
            DECLARE
                r RECORD;
            BEGIN
                FOR r IN 
                    SELECT constraint_name 
                    FROM information_schema.table_constraints 
                    WHERE table_name ILIKE 'AppInventoryTicketLines' 
                      AND constraint_type = 'FOREIGN KEY'
                      AND constraint_name ILIKE 'FK_AppInventoryTicketLines_AppSalesOrderLines%'
                LOOP
                    EXECUTE 'ALTER TABLE ""AppInventoryTicketLines"" DROP CONSTRAINT ""' || r.constraint_name || '""';
                END LOOP;
            END $$;");

        migrationBuilder.Sql(@"
            DO $$
            DECLARE
                r RECORD;
            BEGIN
                FOR r IN 
                    SELECT indexname 
                    FROM pg_indexes 
                    WHERE tablename ILIKE 'AppInventoryTicketLines' 
                      AND indexname ILIKE 'IX_AppInventoryTicketLines_PurchaseOrderLineId%'
                LOOP
                    EXECUTE 'DROP INDEX IF EXISTS ""' || r.indexname || '""';
                END LOOP;
            END $$;");

        migrationBuilder.Sql(@"
            DO $$
            BEGIN
                -- Drop ConcurrencyStamp from AppZones
                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name ILIKE 'AppZones' AND column_name ILIKE 'ConcurrencyStamp') THEN
                    ALTER TABLE ""AppZones"" DROP COLUMN ""ConcurrencyStamp"";
                END IF;
    
                -- Drop ExtraProperties from AppZones
                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name ILIKE 'AppZones' AND column_name ILIKE 'ExtraProperties') THEN
                    ALTER TABLE ""AppZones"" DROP COLUMN ""ExtraProperties"";
                END IF;

                -- Drop PurchaseOrderLineId from AppInventoryTicketLines
                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name ILIKE 'AppInventoryTicketLines' AND column_name ILIKE 'PurchaseOrderLineId') THEN
                    ALTER TABLE ""AppInventoryTicketLines"" DROP COLUMN ""PurchaseOrderLineId"";
                END IF;

                -- Drop ConcurrencyStamp from AppBins
                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name ILIKE 'AppBins' AND column_name ILIKE 'ConcurrencyStamp') THEN
                    ALTER TABLE ""AppBins"" DROP COLUMN ""ConcurrencyStamp"";
                END IF;

                -- Drop ExtraProperties from AppBins
                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name ILIKE 'AppBins' AND column_name ILIKE 'ExtraProperties') THEN
                    ALTER TABLE ""AppBins"" DROP COLUMN ""ExtraProperties"";
                END IF;
            END $$;");

        migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 
                    FROM information_schema.columns 
                    WHERE table_name ILIKE 'AppInventoryTicketLines' 
                      AND column_name ILIKE 'SalesOrderLineId'
                ) THEN
                    ALTER TABLE ""AppInventoryTicketLines"" RENAME COLUMN ""SalesOrderLineId"" TO ""ReferenceDocumentLineId"";
                ELSE
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name ILIKE 'AppInventoryTicketLines' 
                          AND column_name ILIKE 'ReferenceDocumentLineId'
                    ) THEN
                        ALTER TABLE ""AppInventoryTicketLines"" ADD COLUMN ""ReferenceDocumentLineId"" uuid NULL;
                    END IF;
                END IF;
            END $$;");

        migrationBuilder.Sql(@"
            DO $$
            DECLARE
                r RECORD;
            BEGIN
                FOR r IN 
                    SELECT indexname 
                    FROM pg_indexes 
                    WHERE tablename ILIKE 'AppInventoryTicketLines' 
                      AND indexname ILIKE 'IX_AppInventoryTicketLines_SalesOrderLineId%'
                LOOP
                    EXECUTE 'DROP INDEX IF EXISTS ""' || r.indexname || '""';
                END LOOP;
            END $$;");

        migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 
                    FROM pg_indexes 
                    WHERE tablename ILIKE 'AppInventoryTicketLines' 
                      AND indexname ILIKE 'IX_AppInventoryTicketLines_ReferenceDocumentLineId'
                ) THEN
                    CREATE INDEX ""IX_AppInventoryTicketLines_ReferenceDocumentLineId"" ON ""AppInventoryTicketLines"" (""ReferenceDocumentLineId"");
                END IF;
            END $$;");
    }
    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "ReferenceDocumentLineId",
            table: "AppInventoryTicketLines",
            newName: "SalesOrderLineId");

        migrationBuilder.RenameIndex(
            name: "IX_AppInventoryTicketLines_ReferenceDocumentLineId",
            table: "AppInventoryTicketLines",
            newName: "IX_AppInventoryTicketLines_SalesOrderLineId");

        migrationBuilder.AddColumn<string>(
            name: "ConcurrencyStamp",
            table: "AppZones",
            type: "character varying(40)",
            maxLength: 40,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "ExtraProperties",
            table: "AppZones",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<Guid>(
            name: "PurchaseOrderLineId",
            table: "AppInventoryTicketLines",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ConcurrencyStamp",
            table: "AppBins",
            type: "character varying(40)",
            maxLength: 40,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "ExtraProperties",
            table: "AppBins",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "IX_AppInventoryTicketLines_PurchaseOrderLineId",
            table: "AppInventoryTicketLines",
            column: "PurchaseOrderLineId");

        migrationBuilder.AddForeignKey(
            name: "FK_AppInventoryTicketLines_AppPurchaseOrderLines_PurchaseOrder~",
            table: "AppInventoryTicketLines",
            column: "PurchaseOrderLineId",
            principalTable: "AppPurchaseOrderLines",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_AppInventoryTicketLines_AppSalesOrderLines_SalesOrderLineId",
            table: "AppInventoryTicketLines",
            column: "SalesOrderLineId",
            principalTable: "AppSalesOrderLines",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
