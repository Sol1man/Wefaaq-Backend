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

