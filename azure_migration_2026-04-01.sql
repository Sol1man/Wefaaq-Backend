-- =====================================================
-- Azure SQL Database Migration: Add Name Fields
-- Migration ID: 20260401000000_AddNameFieldsToRecordsAndLicenses
-- Date: 2026-04-01
-- AZURE COMPATIBLE VERSION (No GO statements)
-- =====================================================

-- Check if migration already applied
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260401000000_AddNameFieldsToRecordsAndLicenses'
)
BEGIN
    -- Add Name column to OrganizationRecords if not exists
    IF NOT EXISTS (
        SELECT * FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'[OrganizationRecords]') 
        AND name = 'Name'
    )
    BEGIN
        ALTER TABLE [OrganizationRecords] ADD [Name] NVARCHAR(200) NULL;
    END;

    -- Update existing records with default value
    UPDATE [OrganizationRecords] 
    SET [Name] = N'اسم السجل'
    WHERE [Name] IS NULL;

    -- Make the column NOT NULL
    ALTER TABLE [OrganizationRecords] ALTER COLUMN [Name] NVARCHAR(200) NOT NULL;

    -- Add Name column to OrganizationLicenses if not exists
    IF NOT EXISTS (
        SELECT * FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'[OrganizationLicenses]') 
        AND name = 'Name'
    )
    BEGIN
        ALTER TABLE [OrganizationLicenses] ADD [Name] NVARCHAR(200) NULL;
    END;

    -- Update existing licenses with default value
    UPDATE [OrganizationLicenses] 
    SET [Name] = N'اسم الرخصة'
    WHERE [Name] IS NULL;

    -- Make the column NOT NULL
    ALTER TABLE [OrganizationLicenses] ALTER COLUMN [Name] NVARCHAR(200) NOT NULL;

    -- Record migration in history
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260401000000_AddNameFieldsToRecordsAndLicenses', N'8.0.0');

    PRINT 'Migration completed successfully!';
END
ELSE
BEGIN
    PRINT 'Migration already applied - skipping';
END;
