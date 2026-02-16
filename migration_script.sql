IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006161258_InitialCreate'
)
BEGIN
    CREATE TABLE [Clients] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(255) NOT NULL,
        [Email] nvarchar(255) NOT NULL,
        [PhoneNumber] nvarchar(20) NULL,
        [Classification] int NOT NULL,
        [Balance] decimal(18,2) NOT NULL,
        [ExternalWorkersCount] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006161258_InitialCreate'
)
BEGIN
    CREATE TABLE [Organizations] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(255) NOT NULL,
        [CardExpiringSoon] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Organizations] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006161258_InitialCreate'
)
BEGIN
    CREATE TABLE [ClientOrganizations] (
        [ClientId] uniqueidentifier NOT NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ClientOrganizations] PRIMARY KEY ([ClientId], [OrganizationId]),
        CONSTRAINT [FK_ClientOrganizations_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ClientOrganizations_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006161258_InitialCreate'
)
BEGIN
    CREATE TABLE [OrganizationCars] (
        [Id] uniqueidentifier NOT NULL,
        [PlateNumber] nvarchar(20) NOT NULL,
        [Color] nvarchar(50) NOT NULL,
        [SerialNumber] nvarchar(50) NOT NULL,
        [ImagePath] nvarchar(500) NULL,
        [OperatingCardExpiry] datetime2 NOT NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_OrganizationCars] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrganizationCars_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006161258_InitialCreate'
)
BEGIN
    CREATE TABLE [OrganizationLicenses] (
        [Id] uniqueidentifier NOT NULL,
        [Number] nvarchar(100) NOT NULL,
        [ExpiryDate] datetime2 NOT NULL,
        [ImagePath] nvarchar(500) NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_OrganizationLicenses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrganizationLicenses_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006161258_InitialCreate'
)
BEGIN
    CREATE TABLE [OrganizationRecords] (
        [Id] uniqueidentifier NOT NULL,
        [Number] nvarchar(100) NOT NULL,
        [ExpiryDate] datetime2 NOT NULL,
        [ImagePath] nvarchar(500) NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_OrganizationRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrganizationRecords_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006161258_InitialCreate'
)
BEGIN
    CREATE TABLE [OrganizationWorkers] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(255) NOT NULL,
        [ResidenceNumber] nvarchar(50) NOT NULL,
        [ResidenceImagePath] nvarchar(500) NULL,
        [ExpiryDate] datetime2 NOT NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_OrganizationWorkers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrganizationWorkers_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006161258_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ClientOrganizations_OrganizationId] ON [ClientOrganizations] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006161258_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clients_Email] ON [Clients] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006161258_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrganizationCars_OrganizationId] ON [OrganizationCars] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006161258_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrganizationLicenses_OrganizationId] ON [OrganizationLicenses] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006161258_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrganizationRecords_OrganizationId] ON [OrganizationRecords] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006161258_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrganizationWorkers_OrganizationId] ON [OrganizationWorkers] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006161258_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251006161258_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251010192544_UpdateClientOrganizationRelationship'
)
BEGIN
    DROP TABLE [ClientOrganizations];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251010192544_UpdateClientOrganizationRelationship'
)
BEGIN
    ALTER TABLE [Organizations] ADD [ClientId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251010192544_UpdateClientOrganizationRelationship'
)
BEGIN
    CREATE INDEX [IX_Organizations_ClientId] ON [Organizations] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251010192544_UpdateClientOrganizationRelationship'
)
BEGIN
    ALTER TABLE [Organizations] ADD CONSTRAINT [FK_Organizations_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251010192544_UpdateClientOrganizationRelationship'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251010192544_UpdateClientOrganizationRelationship', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251123225029_add-user'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [FirebaseUid] nvarchar(128) NOT NULL,
        [Email] nvarchar(255) NOT NULL,
        [Name] nvarchar(255) NULL,
        [Role] nvarchar(50) NULL,
        [OrganizationId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [LastLoginAt] datetime2 NULL,
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251123225029_add-user'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251123225029_add-user'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_FirebaseUid] ON [Users] ([FirebaseUid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251123225029_add-user'
)
BEGIN
    CREATE INDEX [IX_Users_OrganizationId] ON [Users] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251123225029_add-user'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251123225029_add-user', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    ALTER TABLE [Organizations] DROP CONSTRAINT [FK_Organizations_Clients_ClientId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'ExternalWorkersCount');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Clients] DROP COLUMN [ExternalWorkersCount];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Organizations]') AND [c].[name] = N'ClientId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Organizations] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Organizations] ALTER COLUMN [ClientId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    ALTER TABLE [Organizations] ADD [ClientBranchId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
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
        CONSTRAINT [CK_ExternalWorker_Client_XOR_Branch] CHECK (([ClientId] IS NOT NULL AND [ClientBranchId] IS NULL) OR ([ClientId] IS NULL AND [ClientBranchId] IS NOT NULL)),
        CONSTRAINT [FK_ExternalWorkers_ClientBranches_ClientBranchId] FOREIGN KEY ([ClientBranchId]) REFERENCES [ClientBranches] ([Id]),
        CONSTRAINT [FK_ExternalWorkers_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    CREATE INDEX [IX_Organizations_ClientBranchId] ON [Organizations] ([ClientBranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    EXEC(N'ALTER TABLE [Organizations] ADD CONSTRAINT [CK_Organization_Client_XOR_Branch] CHECK (([ClientId] IS NOT NULL AND [ClientBranchId] IS NULL) OR ([ClientId] IS NULL AND [ClientBranchId] IS NOT NULL))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    CREATE INDEX [IX_ClientBranches_Classification] ON [ClientBranches] ([Classification]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    CREATE INDEX [IX_ClientBranches_Name] ON [ClientBranches] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    CREATE INDEX [IX_ClientBranches_ParentClientId] ON [ClientBranches] ([ParentClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    CREATE INDEX [IX_ExternalWorkers_ClientBranchId] ON [ExternalWorkers] ([ClientBranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    CREATE INDEX [IX_ExternalWorkers_ClientId] ON [ExternalWorkers] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    CREATE INDEX [IX_ExternalWorkers_ExpiryDate] ON [ExternalWorkers] ([ExpiryDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    CREATE INDEX [IX_ExternalWorkers_ResidenceNumber] ON [ExternalWorkers] ([ResidenceNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    CREATE INDEX [IX_ExternalWorkers_WorkerType] ON [ExternalWorkers] ([WorkerType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    CREATE INDEX [IX_OrganizationUsernames_OrganizationId] ON [OrganizationUsernames] ([OrganizationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    CREATE INDEX [IX_OrganizationUsernames_SiteName] ON [OrganizationUsernames] ([SiteName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    ALTER TABLE [Organizations] ADD CONSTRAINT [FK_Organizations_ClientBranches_ClientBranchId] FOREIGN KEY ([ClientBranchId]) REFERENCES [ClientBranches] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    ALTER TABLE [Organizations] ADD CONSTRAINT [FK_Organizations_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114130941_AddExternalWorkersUsernamesAndBranches'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260114130941_AddExternalWorkersUsernamesAndBranches', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [OrganizationWorkers] ADD [DeletedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [OrganizationWorkers] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [OrganizationUsernames] ADD [DeletedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [OrganizationUsernames] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [Organizations] ADD [DeletedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [Organizations] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [OrganizationRecords] ADD [DeletedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [OrganizationRecords] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [OrganizationLicenses] ADD [DeletedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [OrganizationLicenses] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [OrganizationCars] ADD [DeletedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [OrganizationCars] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [ExternalWorkers] ADD [DeletedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [ExternalWorkers] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [Clients] ADD [DeletedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [Clients] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [ClientBranches] ADD [DeletedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    ALTER TABLE [ClientBranches] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114132559_AddSoftDelete'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260114132559_AddSoftDelete', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260114134825_ImplementSoftDelete'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260114134825_ImplementSoftDelete', N'8.0.0');
END;
GO

COMMIT;
GO

