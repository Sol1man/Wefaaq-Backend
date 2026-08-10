-- =====================================================
-- Azure SQL Database Migration: Add Costs table (المصروفات)
-- Migration ID: 20260810120000_AddCostsTable
-- Date: 2026-08-10
-- AZURE COMPATIBLE VERSION (No GO statements)
-- Description: Adds the Costs table — a global, admin-only list of business
--              expenses. Each row has an amount, a description, and a business
--              date (CostDate) used for filtering and the period cards.
--              Soft-delete enabled (IsDeleted / DeletedAt) like the other tables.
-- Previous migration applied in production: 20260804120000_AddProfitPercentageToUser
-- =====================================================

-- Check if migration already applied
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810120000_AddCostsTable'
)
BEGIN
    -- -------------------------------------------------
    -- 1. Create the Costs table
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.tables WHERE name = N'Costs' AND schema_id = SCHEMA_ID(N'dbo')
    )
    BEGIN
        CREATE TABLE [Costs] (
            [Id]          UNIQUEIDENTIFIER  NOT NULL,
            [Amount]      DECIMAL(18,2)     NOT NULL,
            [Description] NVARCHAR(500)     NOT NULL,
            [CostDate]    DATETIME2         NOT NULL,
            [CreatedAt]   DATETIME2         NOT NULL CONSTRAINT [DF_Costs_CreatedAt] DEFAULT (GETUTCDATE()),
            [UpdatedAt]   DATETIME2         NOT NULL CONSTRAINT [DF_Costs_UpdatedAt] DEFAULT (GETUTCDATE()),
            [IsDeleted]   BIT               NOT NULL CONSTRAINT [DF_Costs_IsDeleted] DEFAULT (0),
            [DeletedAt]   DATETIME2         NULL,
            CONSTRAINT [PK_Costs] PRIMARY KEY ([Id])
        );
    END;

    -- -------------------------------------------------
    -- 2. Index on CostDate (filtering + period cards)
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_Costs_CostDate'
        AND object_id = OBJECT_ID(N'[Costs]')
    )
    BEGIN
        CREATE INDEX [IX_Costs_CostDate] ON [Costs] ([CostDate]);
    END;

    -- -------------------------------------------------
    -- 3. Record migration in __EFMigrationsHistory
    -- -------------------------------------------------
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260810120000_AddCostsTable', N'8.0.0');

    PRINT 'Migration 20260810120000_AddCostsTable applied successfully';
    PRINT 'Summary:';
    PRINT '  + Costs table (Id, Amount, Description, CostDate, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)';
    PRINT '  + IX_Costs_CostDate';
END
ELSE
BEGIN
    PRINT 'Migration 20260810120000_AddCostsTable already applied - skipping';
END;
