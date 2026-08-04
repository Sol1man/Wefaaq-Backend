-- =====================================================
-- Azure SQL Database Migration: Client Operations — CustomType
-- Migration ID: 20260602103722_AddCustomTypeToClientOperation
-- Date: 2026-06-02
-- AZURE COMPATIBLE VERSION (No GO statements)
-- Description: Adds the free-text CustomType column to ClientOperations,
--              used only when Type = Other (نوع مخصص). Nullable.
-- Previous migration applied in production: 20260510153613_AddPaymentTypeAndAccountAmounts
-- NOTE: This migration was never deployed to production; run it BEFORE
--       azure_migration_2026-06-10.sql (RemoveOperationStatusAddKind).
-- =====================================================

-- Check if migration already applied
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260602103722_AddCustomTypeToClientOperation'
)
BEGIN
    -- -------------------------------------------------
    -- 1. ClientOperations: add CustomType (free-text, used when Type = Other)
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.columns
        WHERE object_id = OBJECT_ID(N'[ClientOperations]')
        AND name = 'CustomType'
    )
    BEGIN
        ALTER TABLE [ClientOperations]
            ADD [CustomType] NVARCHAR(255) NULL;
    END;

    -- -------------------------------------------------
    -- 2. Record migration in __EFMigrationsHistory
    -- -------------------------------------------------
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260602103722_AddCustomTypeToClientOperation', N'8.0.0');

    PRINT 'Migration 20260602103722_AddCustomTypeToClientOperation applied successfully';
    PRINT 'Summary:';
    PRINT '  + ClientOperations.CustomType  nvarchar(255) NULL';
END
ELSE
BEGIN
    PRINT 'Migration 20260602103722_AddCustomTypeToClientOperation already applied - skipping';
END;
