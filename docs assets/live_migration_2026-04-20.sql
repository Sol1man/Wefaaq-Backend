-- =====================================================
-- Production Migration: Add Client Operations Table
-- Migration ID: 20260411151016_AddClientOperations
-- Date: 2026-04-20
-- Description: Adds the ClientOperations table to track operations performed
--              on clients, branches, organizations, or external persons.
-- =====================================================

BEGIN TRANSACTION;
GO

-- =====================================================
-- Step 1: Create ClientOperations table
-- =====================================================
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411151016_AddClientOperations'
)
BEGIN
    IF NOT EXISTS (
        SELECT * FROM sys.objects
        WHERE object_id = OBJECT_ID(N'[ClientOperations]')
        AND type = N'U'
    )
    BEGIN
        CREATE TABLE [ClientOperations] (
            [Id] uniqueidentifier NOT NULL,
            [Type] int NOT NULL,
            [TargetType] int NOT NULL,
            [Status] int NOT NULL,
            [Price] decimal(18,2) NULL,
            [Notes] nvarchar(1000) NULL,
            [ClientId] uniqueidentifier NULL,
            [ClientBranchId] uniqueidentifier NULL,
            [OrganizationId] uniqueidentifier NULL,
            [ExternalPersonName] nvarchar(255) NULL,
            [ExternalPersonIdNumber] nvarchar(50) NULL,
            [PerformedByUserId] int NOT NULL,
            [CompletedAt] datetime2 NULL,
            [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
            [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
            [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
            [DeletedAt] datetime2 NULL,
            CONSTRAINT [PK_ClientOperations] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_ClientOperations_ClientBranches_ClientBranchId]
                FOREIGN KEY ([ClientBranchId]) REFERENCES [ClientBranches] ([Id]),
            CONSTRAINT [FK_ClientOperations_Clients_ClientId]
                FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]),
            CONSTRAINT [FK_ClientOperations_Organizations_OrganizationId]
                FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]),
            CONSTRAINT [FK_ClientOperations_Users_PerformedByUserId]
                FOREIGN KEY ([PerformedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
        );
        PRINT 'Created ClientOperations table';
    END
    ELSE
    BEGIN
        PRINT 'ClientOperations table already exists, skipping creation';
    END
END;
GO

-- =====================================================
-- Step 2: Create indexes on ClientOperations
-- =====================================================
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411151016_AddClientOperations'
)
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClientOperations_ClientBranchId' AND object_id = OBJECT_ID(N'[ClientOperations]'))
        CREATE INDEX [IX_ClientOperations_ClientBranchId] ON [ClientOperations] ([ClientBranchId]);

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClientOperations_ClientId' AND object_id = OBJECT_ID(N'[ClientOperations]'))
        CREATE INDEX [IX_ClientOperations_ClientId] ON [ClientOperations] ([ClientId]);

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClientOperations_CreatedAt' AND object_id = OBJECT_ID(N'[ClientOperations]'))
        CREATE INDEX [IX_ClientOperations_CreatedAt] ON [ClientOperations] ([CreatedAt]);

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClientOperations_OrganizationId' AND object_id = OBJECT_ID(N'[ClientOperations]'))
        CREATE INDEX [IX_ClientOperations_OrganizationId] ON [ClientOperations] ([OrganizationId]);

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClientOperations_PerformedByUserId' AND object_id = OBJECT_ID(N'[ClientOperations]'))
        CREATE INDEX [IX_ClientOperations_PerformedByUserId] ON [ClientOperations] ([PerformedByUserId]);

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClientOperations_Status' AND object_id = OBJECT_ID(N'[ClientOperations]'))
        CREATE INDEX [IX_ClientOperations_Status] ON [ClientOperations] ([Status]);

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClientOperations_Type' AND object_id = OBJECT_ID(N'[ClientOperations]'))
        CREATE INDEX [IX_ClientOperations_Type] ON [ClientOperations] ([Type]);

    PRINT 'Created indexes on ClientOperations';
END;
GO

-- =====================================================
-- Step 3: Record migration in history
-- =====================================================
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411151016_AddClientOperations'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260411151016_AddClientOperations', N'8.0.0');
    PRINT 'Migration recorded in __EFMigrationsHistory';
END;
GO

COMMIT;
GO

PRINT '===== Migration completed successfully =====';
PRINT 'Summary:';
PRINT '- Created ClientOperations table with all FKs and indexes';
PRINT '- Migration tracked in __EFMigrationsHistory as 20260411151016_AddClientOperations';
PRINT '============================================';
GO
