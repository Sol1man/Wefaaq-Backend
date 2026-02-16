-- Safe Migration Script for Production
-- Checks for existence before making changes

-- =====================================================
-- Migration: AddExternalWorkersUsernamesAndBranches
-- =====================================================

-- Drop FK constraint if it exists
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Organizations_Clients_ClientId')
BEGIN
    ALTER TABLE [Organizations] DROP CONSTRAINT [FK_Organizations_Clients_ClientId];
END
GO

-- Drop ExternalWorkersCount column if it exists
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Clients]') AND name = N'ExternalWorkersCount')
BEGIN
    -- Drop check constraint if exists
    IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Clients_ExternalWorkersCount')
        ALTER TABLE [Clients] DROP CONSTRAINT [CK_Clients_ExternalWorkersCount];

    -- Drop any other check constraints on this column
    DECLARE @ckName sysname;
    SELECT @ckName = cc.name
    FROM sys.check_constraints cc
    INNER JOIN sys.columns c ON cc.parent_object_id = c.object_id
    WHERE cc.parent_object_id = OBJECT_ID(N'[Clients]')
      AND c.name = N'ExternalWorkersCount'
      AND cc.definition LIKE '%ExternalWorkersCount%';
    IF @ckName IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @ckName + '];');

    -- Drop default constraint if exists
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'ExternalWorkersCount');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var0 + '];');

    ALTER TABLE [Clients] DROP COLUMN [ExternalWorkersCount];
END
GO

-- Make ClientId nullable if not already
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Organizations]') AND name = N'ClientId' AND is_nullable = 0)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Organizations]') AND [c].[name] = N'ClientId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Organizations] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Organizations] ALTER COLUMN [ClientId] uniqueidentifier NULL;
END
GO

-- Add ClientBranchId column if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Organizations]') AND name = N'ClientBranchId')
BEGIN
    ALTER TABLE [Organizations] ADD [ClientBranchId] uniqueidentifier NULL;
END
GO

-- Create ClientBranches table if not exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ClientBranches')
BEGIN
    CREATE TABLE [ClientBranches] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(255) NOT NULL,
        [Email] nvarchar(255) NULL,
        [PhoneNumber] nvarchar(20) NULL,
        [Classification] int NOT NULL,
        [Balance] decimal(18,2) NOT NULL,
        [BranchType] nvarchar(100) NULL,
        [ParentClientId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_ClientBranches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClientBranches_Clients_ParentClientId] FOREIGN KEY ([ParentClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
    );
END
GO

-- Create OrganizationUsernames table if not exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrganizationUsernames')
BEGIN
    CREATE TABLE [OrganizationUsernames] (
        [Id] uniqueidentifier NOT NULL,
        [SiteName] nvarchar(200) NOT NULL,
        [Username] nvarchar(255) NOT NULL,
        [Password] nvarchar(500) NOT NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_OrganizationUsernames] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrganizationUsernames_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE CASCADE
    );
END
GO

-- Create ExternalWorkers table if not exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExternalWorkers')
BEGIN
    CREATE TABLE [ExternalWorkers] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(255) NOT NULL,
        [ResidenceNumber] nvarchar(50) NOT NULL,
        [ResidenceImagePath] nvarchar(500) NULL,
        [ExpiryDate] datetime2 NOT NULL,
        [WorkerType] int NOT NULL,
        [ClientId] uniqueidentifier NULL,
        [ClientBranchId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_ExternalWorkers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExternalWorkers_ClientBranches_ClientBranchId] FOREIGN KEY ([ClientBranchId]) REFERENCES [ClientBranches] ([Id]),
        CONSTRAINT [FK_ExternalWorkers_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id])
    );
END
GO

-- Create indexes if not exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Organizations_ClientBranchId' AND object_id = OBJECT_ID('Organizations'))
    CREATE INDEX [IX_Organizations_ClientBranchId] ON [Organizations] ([ClientBranchId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClientBranches_Classification' AND object_id = OBJECT_ID('ClientBranches'))
    CREATE INDEX [IX_ClientBranches_Classification] ON [ClientBranches] ([Classification]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClientBranches_Name' AND object_id = OBJECT_ID('ClientBranches'))
    CREATE INDEX [IX_ClientBranches_Name] ON [ClientBranches] ([Name]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClientBranches_ParentClientId' AND object_id = OBJECT_ID('ClientBranches'))
    CREATE INDEX [IX_ClientBranches_ParentClientId] ON [ClientBranches] ([ParentClientId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExternalWorkers_ClientBranchId' AND object_id = OBJECT_ID('ExternalWorkers'))
    CREATE INDEX [IX_ExternalWorkers_ClientBranchId] ON [ExternalWorkers] ([ClientBranchId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExternalWorkers_ClientId' AND object_id = OBJECT_ID('ExternalWorkers'))
    CREATE INDEX [IX_ExternalWorkers_ClientId] ON [ExternalWorkers] ([ClientId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExternalWorkers_ExpiryDate' AND object_id = OBJECT_ID('ExternalWorkers'))
    CREATE INDEX [IX_ExternalWorkers_ExpiryDate] ON [ExternalWorkers] ([ExpiryDate]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExternalWorkers_ResidenceNumber' AND object_id = OBJECT_ID('ExternalWorkers'))
    CREATE INDEX [IX_ExternalWorkers_ResidenceNumber] ON [ExternalWorkers] ([ResidenceNumber]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExternalWorkers_WorkerType' AND object_id = OBJECT_ID('ExternalWorkers'))
    CREATE INDEX [IX_ExternalWorkers_WorkerType] ON [ExternalWorkers] ([WorkerType]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OrganizationUsernames_OrganizationId' AND object_id = OBJECT_ID('OrganizationUsernames'))
    CREATE INDEX [IX_OrganizationUsernames_OrganizationId] ON [OrganizationUsernames] ([OrganizationId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OrganizationUsernames_SiteName' AND object_id = OBJECT_ID('OrganizationUsernames'))
    CREATE INDEX [IX_OrganizationUsernames_SiteName] ON [OrganizationUsernames] ([SiteName]);
GO

-- Add FK constraints if not exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Organizations_ClientBranches_ClientBranchId')
    ALTER TABLE [Organizations] ADD CONSTRAINT [FK_Organizations_ClientBranches_ClientBranchId] FOREIGN KEY ([ClientBranchId]) REFERENCES [ClientBranches] ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Organizations_Clients_ClientId')
    ALTER TABLE [Organizations] ADD CONSTRAINT [FK_Organizations_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]);
GO

-- Record migration if not exists
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260114130941_AddExternalWorkersUsernamesAndBranches', N'8.0.0');
END
GO

-- =====================================================
-- Migration: AddSoftDelete
-- =====================================================

-- Add soft delete columns to OrganizationWorkers
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrganizationWorkers]') AND name = N'DeletedAt')
    ALTER TABLE [OrganizationWorkers] ADD [DeletedAt] datetime2 NULL;
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrganizationWorkers]') AND name = N'IsDeleted')
    ALTER TABLE [OrganizationWorkers] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

-- Add soft delete columns to OrganizationUsernames
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrganizationUsernames]') AND name = N'DeletedAt')
    ALTER TABLE [OrganizationUsernames] ADD [DeletedAt] datetime2 NULL;
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrganizationUsernames]') AND name = N'IsDeleted')
    ALTER TABLE [OrganizationUsernames] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

-- Add soft delete columns to Organizations
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Organizations]') AND name = N'DeletedAt')
    ALTER TABLE [Organizations] ADD [DeletedAt] datetime2 NULL;
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Organizations]') AND name = N'IsDeleted')
    ALTER TABLE [Organizations] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

-- Add soft delete columns to OrganizationRecords
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrganizationRecords]') AND name = N'DeletedAt')
    ALTER TABLE [OrganizationRecords] ADD [DeletedAt] datetime2 NULL;
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrganizationRecords]') AND name = N'IsDeleted')
    ALTER TABLE [OrganizationRecords] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

-- Add soft delete columns to OrganizationLicenses
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrganizationLicenses]') AND name = N'DeletedAt')
    ALTER TABLE [OrganizationLicenses] ADD [DeletedAt] datetime2 NULL;
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrganizationLicenses]') AND name = N'IsDeleted')
    ALTER TABLE [OrganizationLicenses] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

-- Add soft delete columns to OrganizationCars
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrganizationCars]') AND name = N'DeletedAt')
    ALTER TABLE [OrganizationCars] ADD [DeletedAt] datetime2 NULL;
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[OrganizationCars]') AND name = N'IsDeleted')
    ALTER TABLE [OrganizationCars] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

-- Add soft delete columns to ExternalWorkers
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[ExternalWorkers]') AND name = N'DeletedAt')
    ALTER TABLE [ExternalWorkers] ADD [DeletedAt] datetime2 NULL;
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[ExternalWorkers]') AND name = N'IsDeleted')
    ALTER TABLE [ExternalWorkers] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

-- Add soft delete columns to Clients
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Clients]') AND name = N'DeletedAt')
    ALTER TABLE [Clients] ADD [DeletedAt] datetime2 NULL;
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Clients]') AND name = N'IsDeleted')
    ALTER TABLE [Clients] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

-- Add soft delete columns to ClientBranches
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[ClientBranches]') AND name = N'DeletedAt')
    ALTER TABLE [ClientBranches] ADD [DeletedAt] datetime2 NULL;
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[ClientBranches]') AND name = N'IsDeleted')
    ALTER TABLE [ClientBranches] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

-- Record migration if not exists
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260114132559_AddSoftDelete')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260114132559_AddSoftDelete', N'8.0.0');
END
GO

-- =====================================================
-- Migration: ImplementSoftDelete
-- =====================================================

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260114134825_ImplementSoftDelete')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260114134825_ImplementSoftDelete', N'8.0.0');
END
GO

-- =====================================================
-- Seed Admin User (only if not exists)
-- =====================================================

IF NOT EXISTS (SELECT * FROM [Users] WHERE [FirebaseUid] = '3dMt71HRn7gBLiTYpHgy9UtKyWX2')
BEGIN
    SET IDENTITY_INSERT [Users] ON;
    INSERT INTO [Users] ([Id], [FirebaseUid], [Email], [Name], [Role], [OrganizationId], [IsActive], [CreatedAt], [LastLoginAt], [UpdatedAt])
    VALUES (1, '3dMt71HRn7gBLiTYpHgy9UtKyWX2', 'admin@wefaaq.com', NULL, 'Administrator', NULL, 1, GETUTCDATE(), NULL, GETUTCDATE());
    SET IDENTITY_INSERT [Users] OFF;
    PRINT 'Admin user seeded successfully!';
END
ELSE
BEGIN
    PRINT 'Admin user already exists, skipping...';
END
GO

PRINT 'Migration completed successfully!';
GO
