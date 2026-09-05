-- =====================================================
-- Azure SQL Database Migration: Employee salaries (رواتب الموظفين)
-- Migration ID: 20260905164935_AddEmployeeSalaries
-- Date: 2026-09-05
-- AZURE COMPATIBLE VERSION (No GO statements)
-- Description: Adds the employee salaries feature.
--              1. Users.Salary          — monthly salary for system users
--              2. ExternalEmployees     — payroll-only people with no system account
--              3. EmployeeDeductions    — salary deductions, belonging to either a
--                                        User or an ExternalEmployee (XOR constraint)
--              Soft-delete enabled (IsDeleted / DeletedAt) like the other tables.
-- Previous migration applied in production: 20260810120000_AddCostsTable
-- =====================================================

-- Check if migration already applied
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905164935_AddEmployeeSalaries'
)
BEGIN
    -- -------------------------------------------------
    -- 1. Users.Salary column
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.columns
        WHERE name = N'Salary' AND object_id = OBJECT_ID(N'[Users]')
    )
    BEGIN
        ALTER TABLE [Users]
        ADD [Salary] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_Users_Salary] DEFAULT (0);
    END;

    -- -------------------------------------------------
    -- 2. ExternalEmployees table
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.tables WHERE name = N'ExternalEmployees' AND schema_id = SCHEMA_ID(N'dbo')
    )
    BEGIN
        CREATE TABLE [ExternalEmployees] (
            [Id]        UNIQUEIDENTIFIER NOT NULL,
            [Name]      NVARCHAR(255)    NOT NULL,
            [Salary]    DECIMAL(18,2)    NOT NULL CONSTRAINT [DF_ExternalEmployees_Salary]    DEFAULT (0),
            [CreatedAt] DATETIME2        NOT NULL CONSTRAINT [DF_ExternalEmployees_CreatedAt] DEFAULT (GETUTCDATE()),
            [UpdatedAt] DATETIME2        NOT NULL CONSTRAINT [DF_ExternalEmployees_UpdatedAt] DEFAULT (GETUTCDATE()),
            [IsDeleted] BIT              NOT NULL CONSTRAINT [DF_ExternalEmployees_IsDeleted] DEFAULT (0),
            [DeletedAt] DATETIME2        NULL,
            CONSTRAINT [PK_ExternalEmployees] PRIMARY KEY ([Id])
        );
    END;

    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_ExternalEmployees_Name'
        AND object_id = OBJECT_ID(N'[ExternalEmployees]')
    )
    BEGIN
        CREATE INDEX [IX_ExternalEmployees_Name] ON [ExternalEmployees] ([Name]);
    END;

    -- -------------------------------------------------
    -- 3. EmployeeDeductions table
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.tables WHERE name = N'EmployeeDeductions' AND schema_id = SCHEMA_ID(N'dbo')
    )
    BEGIN
        CREATE TABLE [EmployeeDeductions] (
            [Id]                 UNIQUEIDENTIFIER NOT NULL,
            [Amount]             DECIMAL(18,2)    NOT NULL,
            [Description]        NVARCHAR(500)    NOT NULL,
            [DeductionDate]      DATETIME2        NOT NULL,
            [UserId]             INT              NULL,
            [ExternalEmployeeId] UNIQUEIDENTIFIER NULL,
            [CreatedAt]          DATETIME2        NOT NULL CONSTRAINT [DF_EmployeeDeductions_CreatedAt] DEFAULT (GETUTCDATE()),
            [UpdatedAt]          DATETIME2        NOT NULL CONSTRAINT [DF_EmployeeDeductions_UpdatedAt] DEFAULT (GETUTCDATE()),
            [IsDeleted]          BIT              NOT NULL CONSTRAINT [DF_EmployeeDeductions_IsDeleted] DEFAULT (0),
            [DeletedAt]          DATETIME2        NULL,
            CONSTRAINT [PK_EmployeeDeductions] PRIMARY KEY ([Id]),
            CONSTRAINT [CK_EmployeeDeduction_User_XOR_External] CHECK (
                ([UserId] IS NOT NULL AND [ExternalEmployeeId] IS NULL)
                OR ([UserId] IS NULL AND [ExternalEmployeeId] IS NOT NULL)
            ),
            CONSTRAINT [FK_EmployeeDeductions_ExternalEmployees_ExternalEmployeeId]
                FOREIGN KEY ([ExternalEmployeeId]) REFERENCES [ExternalEmployees] ([Id]) ON DELETE CASCADE,
            CONSTRAINT [FK_EmployeeDeductions_Users_UserId]
                FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
        );
    END;

    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_EmployeeDeductions_DeductionDate'
        AND object_id = OBJECT_ID(N'[EmployeeDeductions]')
    )
    BEGIN
        CREATE INDEX [IX_EmployeeDeductions_DeductionDate] ON [EmployeeDeductions] ([DeductionDate]);
    END;

    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_EmployeeDeductions_ExternalEmployeeId'
        AND object_id = OBJECT_ID(N'[EmployeeDeductions]')
    )
    BEGIN
        CREATE INDEX [IX_EmployeeDeductions_ExternalEmployeeId] ON [EmployeeDeductions] ([ExternalEmployeeId]);
    END;

    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_EmployeeDeductions_UserId'
        AND object_id = OBJECT_ID(N'[EmployeeDeductions]')
    )
    BEGIN
        CREATE INDEX [IX_EmployeeDeductions_UserId] ON [EmployeeDeductions] ([UserId]);
    END;

    -- -------------------------------------------------
    -- 4. Record migration in __EFMigrationsHistory
    -- -------------------------------------------------
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905164935_AddEmployeeSalaries', N'8.0.0');

    PRINT 'Migration 20260905164935_AddEmployeeSalaries applied successfully';
    PRINT 'Summary:';
    PRINT '  + Users.Salary (decimal(18,2), default 0)';
    PRINT '  + ExternalEmployees table + IX_ExternalEmployees_Name';
    PRINT '  + EmployeeDeductions table + XOR check constraint + 3 indexes';
END
ELSE
BEGIN
    PRINT 'Migration 20260905164935_AddEmployeeSalaries already applied - skipping';
END;
