-- =====================================================
-- Production Migration: Add Name Fields to Records and Licenses
-- Migration ID: 20260401000000_AddNameFieldsToRecordsAndLicenses
-- Date: 2026-04-01
-- Description: Adds Name field to OrganizationRecords and OrganizationLicenses tables with default values for existing records
-- =====================================================

BEGIN TRANSACTION;
GO

-- =====================================================
-- Step 1: Add Name column to OrganizationRecords
-- =====================================================
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260401000000_AddNameFieldsToRecordsAndLicenses'
)
BEGIN
    -- First, add the column as nullable
    IF NOT EXISTS (
        SELECT * FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'[OrganizationRecords]') 
        AND name = 'Name'
    )
    BEGIN
        ALTER TABLE [OrganizationRecords] ADD [Name] NVARCHAR(200) NULL;
        PRINT 'Added Name column to OrganizationRecords';
    END

    -- Set default value for existing records (using Arabic default)
    UPDATE [OrganizationRecords] 
    SET [Name] = N'اسم السجل'
    WHERE [Name] IS NULL;
    PRINT 'Updated existing OrganizationRecords with default names';

    -- Make the column NOT NULL
    ALTER TABLE [OrganizationRecords] ALTER COLUMN [Name] NVARCHAR(200) NOT NULL;
    PRINT 'Made Name column NOT NULL in OrganizationRecords';
END;
GO

-- =====================================================
-- Step 2: Add Name column to OrganizationLicenses
-- =====================================================
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260401000000_AddNameFieldsToRecordsAndLicenses'
)
BEGIN
    -- First, add the column as nullable
    IF NOT EXISTS (
        SELECT * FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'[OrganizationLicenses]') 
        AND name = 'Name'
    )
    BEGIN
        ALTER TABLE [OrganizationLicenses] ADD [Name] NVARCHAR(200) NULL;
        PRINT 'Added Name column to OrganizationLicenses';
    END

    -- Set default value for existing licenses (using Arabic default)
    UPDATE [OrganizationLicenses] 
    SET [Name] = N'اسم الرخصة'
    WHERE [Name] IS NULL;
    PRINT 'Updated existing OrganizationLicenses with default names';

    -- Make the column NOT NULL
    ALTER TABLE [OrganizationLicenses] ALTER COLUMN [Name] NVARCHAR(200) NOT NULL;
    PRINT 'Made Name column NOT NULL in OrganizationLicenses';
END;
GO

-- =====================================================
-- Step 3: Record migration in history
-- =====================================================
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260401000000_AddNameFieldsToRecordsAndLicenses'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260401000000_AddNameFieldsToRecordsAndLicenses', N'8.0.0');
    PRINT 'Migration recorded in __EFMigrationsHistory';
END;
GO

COMMIT;
GO

PRINT '===== Migration completed successfully =====';
PRINT 'Summary:';
PRINT '- Added Name field to OrganizationRecords with default value: اسم السجل';
PRINT '- Added Name field to OrganizationLicenses with default value: اسم الرخصة';
PRINT '- All existing records/licenses now have default names';
PRINT '============================================';
GO
