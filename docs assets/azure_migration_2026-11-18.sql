-- =====================================================
-- Azure SQL Database Migration: Add UserPagePermissions table (صلاحيات المستخدمين)
-- Migration ID: 20261118202228_AddUserPagePermissions
-- Date: 2026-11-18
-- AZURE COMPATIBLE VERSION (No GO statements)
-- Description: Per-user page permissions managed by the admin from the new
--              "User Permissions" screen. One row per user per page with three flags:
--              CanView / CanAddEdit / CanDelete.
--              NO DATA IS SEEDED ON PURPOSE: a user with no rows for a page gets that
--              page's default, and the defaults reproduce exactly what the "User" role
--              could do before this release. So nothing changes for any existing user
--              until an admin edits them. Admins ignore this table (always full access).
--              Rows cascade-delete with their user.
-- Previous migration applied in production: 20261013095731_AddRents
-- =====================================================

-- Check if migration already applied
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118202228_AddUserPagePermissions'
)
BEGIN
    -- -------------------------------------------------
    -- 1. Create the UserPagePermissions table
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.tables WHERE name = N'UserPagePermissions' AND schema_id = SCHEMA_ID(N'dbo')
    )
    BEGIN
        CREATE TABLE [UserPagePermissions] (
            [Id]         INT IDENTITY(1,1) NOT NULL,
            [UserId]     INT               NOT NULL,
            [PageKey]    NVARCHAR(50)      NOT NULL,
            [CanView]    BIT               NOT NULL,
            [CanAddEdit] BIT               NOT NULL,
            [CanDelete]  BIT               NOT NULL,
            [UpdatedAt]  DATETIME2         NOT NULL CONSTRAINT [DF_UserPagePermissions_UpdatedAt] DEFAULT (GETUTCDATE()),
            CONSTRAINT [PK_UserPagePermissions] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_UserPagePermissions_Users_UserId]
                FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
        );
    END;

    -- -------------------------------------------------
    -- 2. One row per user per page
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_UserPagePermissions_UserId_PageKey' AND object_id = OBJECT_ID(N'[UserPagePermissions]')
    )
    BEGIN
        CREATE UNIQUE INDEX [IX_UserPagePermissions_UserId_PageKey]
            ON [UserPagePermissions] ([UserId], [PageKey]);
    END;

    -- -------------------------------------------------
    -- 3. Record migration in __EFMigrationsHistory
    -- -------------------------------------------------
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261118202228_AddUserPagePermissions', N'8.0.0');

    PRINT 'Migration 20261118202228_AddUserPagePermissions applied successfully';
    PRINT 'Summary:';
    PRINT '  + UserPagePermissions table (Id, UserId, PageKey, CanView, CanAddEdit, CanDelete, UpdatedAt)';
    PRINT '  + FK_UserPagePermissions_Users_UserId (cascade)';
    PRINT '  + IX_UserPagePermissions_UserId_PageKey (unique)';
END
ELSE
BEGIN
    PRINT 'Migration 20261118202228_AddUserPagePermissions already applied - skipping';
END;

-- =====================================================
-- ROLLBACK (only if you must undo this release; deletes every saved permission)
-- =====================================================
-- DROP TABLE IF EXISTS [UserPagePermissions];
-- DELETE FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261118202228_AddUserPagePermissions';
