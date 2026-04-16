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

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207141958_AddRolesTable'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(255) NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207141958_AddRolesTable'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Roles]'))
        SET IDENTITY_INSERT [Roles] ON;
    EXEC(N'INSERT INTO [Roles] ([Id], [Description], [Name])
    VALUES (1, N''Full system access'', N''Admin''),
    (2, N''Standard user access'', N''User'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Roles]'))
        SET IDENTITY_INSERT [Roles] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207141958_AddRolesTable'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_Name] ON [Roles] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207141958_AddRolesTable'
)
BEGIN
    ALTER TABLE [Users] ADD [RoleId] int NOT NULL DEFAULT 2;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207141958_AddRolesTable'
)
BEGIN
                    UPDATE Users
                    SET RoleId = 1
                    WHERE Role = 'Admin' OR Role = 'Administrator'
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207141958_AddRolesTable'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Role');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Users] DROP COLUMN [Role];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207141958_AddRolesTable'
)
BEGIN
    CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207141958_AddRolesTable'
)
BEGIN
    ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207141958_AddRolesTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260207141958_AddRolesTable', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207145805_AddUserPaymentTable'
)
BEGIN
    CREATE TABLE [UserPayments] (
        [Id] uniqueidentifier NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [UserId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_UserPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserPayments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207145805_AddUserPaymentTable'
)
BEGIN
    CREATE INDEX [IX_UserPayments_CreatedAt] ON [UserPayments] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207145805_AddUserPaymentTable'
)
BEGIN
    CREATE INDEX [IX_UserPayments_UserId] ON [UserPayments] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207145805_AddUserPaymentTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260207145805_AddUserPaymentTable', N'8.0.0');
END;
GO

COMMIT;
GO

-- =====================================================
-- Insert Users
-- =====================================================
BEGIN TRANSACTION;
GO

-- Admin user
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [FirebaseUid] = N'Brsrt3fOOYbQuM1znKl4Y1TnauI3')
BEGIN
    INSERT INTO [Users] ([FirebaseUid], [Email], [Name], [RoleId], [OrganizationId], [IsActive], [CreatedAt], [LastLoginAt], [UpdatedAt])
    VALUES (N'Brsrt3fOOYbQuM1znKl4Y1TnauI3', N'Maktab.edad11@gmail.com', N'Maktab Edad', 1, NULL, 1, GETUTCDATE(), NULL, GETUTCDATE());
END;
GO

-- Dev user
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [FirebaseUid] = N'aPxELFZN2NZsUffgxniwb5eYq463')
BEGIN
    INSERT INTO [Users] ([FirebaseUid], [Email], [Name], [RoleId], [OrganizationId], [IsActive], [CreatedAt], [LastLoginAt], [UpdatedAt])
    VALUES (N'aPxELFZN2NZsUffgxniwb5eYq463', N'devuser@wefaaq.com', N'Dev User', 2, NULL, 1, GETUTCDATE(), NULL, GETUTCDATE());
END;
GO

COMMIT;
GO

