-- =====================================================
-- Azure SQL Database Migration: Add ProfitPercentage to Users
-- Migration ID: 20260804120000_AddProfitPercentageToUser
-- Date: 2026-08-04
-- AZURE COMPATIBLE VERSION (No GO statements)
-- Description: Adds a per-user profit-share percentage (0-100). The admin sets
--              it alongside the account top-up; each user's cut is
--              ProfitPercentage% of their profit total for the selected period.
--              Default 0 backfills every existing user with no profit share.
-- Previous migration applied in production: 20260610211701_RemoveOperationStatusAddKind
-- =====================================================

-- Check if migration already applied
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260804120000_AddProfitPercentageToUser'
)
BEGIN
    -- -------------------------------------------------
    -- 1. Add ProfitPercentage (decimal(5,2), NOT NULL, default 0)
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.columns
        WHERE object_id = OBJECT_ID(N'[Users]')
        AND name = 'ProfitPercentage'
    )
    BEGIN
        ALTER TABLE [Users]
            ADD [ProfitPercentage] DECIMAL(5,2) NOT NULL
            CONSTRAINT [DF_Users_ProfitPercentage] DEFAULT (0);
    END;

    -- -------------------------------------------------
    -- 2. Record migration in __EFMigrationsHistory
    -- -------------------------------------------------
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260804120000_AddProfitPercentageToUser', N'8.0.0');

    PRINT 'Migration 20260804120000_AddProfitPercentageToUser applied successfully';
    PRINT 'Summary:';
    PRINT '  + Users.ProfitPercentage          decimal(5,2) NOT NULL default 0';
END
ELSE
BEGIN
    PRINT 'Migration 20260804120000_AddProfitPercentageToUser already applied - skipping';
END;
