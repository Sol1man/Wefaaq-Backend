-- =====================================================
-- Azure SQL Database Migration: Add EmployeeLoans table (سلف الموظفين)
-- Migration ID: 20260907200128_AddEmployeeLoans
-- Date: 2026-09-09
-- AZURE COMPATIBLE VERSION (No GO statements)
-- Description: Adds the EmployeeLoans table — money the company advances to an
--              employee. Carries no profit and no interest: the amount is simply
--              subtracted from that month's salary, exactly like a deduction,
--              but stored separately so the two histories never mix.
--              Belongs to either a User or an ExternalEmployee, never both
--              (XOR check constraint), mirroring EmployeeDeductions.
--              Soft-delete enabled (IsDeleted / DeletedAt) like the other tables.
-- Previous migration applied in production: 20260905164935_AddEmployeeSalaries
-- =====================================================

-- Check if migration already applied
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907200128_AddEmployeeLoans'
)
BEGIN
    -- -------------------------------------------------
    -- 1. Create the EmployeeLoans table
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.tables WHERE name = N'EmployeeLoans' AND schema_id = SCHEMA_ID(N'dbo')
    )
    BEGIN
        CREATE TABLE [EmployeeLoans] (
            [Id]                 UNIQUEIDENTIFIER NOT NULL,
            [Amount]             DECIMAL(18,2)    NOT NULL,
            [Description]        NVARCHAR(500)    NOT NULL,
            [LoanDate]           DATETIME2        NOT NULL,
            [UserId]             INT              NULL,
            [ExternalEmployeeId] UNIQUEIDENTIFIER NULL,
            [CreatedAt]          DATETIME2        NOT NULL CONSTRAINT [DF_EmployeeLoans_CreatedAt] DEFAULT (GETUTCDATE()),
            [UpdatedAt]          DATETIME2        NOT NULL CONSTRAINT [DF_EmployeeLoans_UpdatedAt] DEFAULT (GETUTCDATE()),
            [IsDeleted]          BIT              NOT NULL CONSTRAINT [DF_EmployeeLoans_IsDeleted] DEFAULT (0),
            [DeletedAt]          DATETIME2        NULL,
            CONSTRAINT [PK_EmployeeLoans] PRIMARY KEY ([Id]),
            CONSTRAINT [CK_EmployeeLoan_User_XOR_External] CHECK (
                ([UserId] IS NOT NULL AND [ExternalEmployeeId] IS NULL)
                OR ([UserId] IS NULL AND [ExternalEmployeeId] IS NOT NULL)
            ),
            CONSTRAINT [FK_EmployeeLoans_ExternalEmployees_ExternalEmployeeId]
                FOREIGN KEY ([ExternalEmployeeId]) REFERENCES [ExternalEmployees] ([Id]) ON DELETE CASCADE,
            CONSTRAINT [FK_EmployeeLoans_Users_UserId]
                FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
        );
    END;

    -- -------------------------------------------------
    -- 2. Index on LoanDate (monthly filtering + period cards)
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_EmployeeLoans_LoanDate'
        AND object_id = OBJECT_ID(N'[EmployeeLoans]')
    )
    BEGIN
        CREATE INDEX [IX_EmployeeLoans_LoanDate] ON [EmployeeLoans] ([LoanDate]);
    END;

    -- -------------------------------------------------
    -- 3. Index on ExternalEmployeeId (owner lookup)
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_EmployeeLoans_ExternalEmployeeId'
        AND object_id = OBJECT_ID(N'[EmployeeLoans]')
    )
    BEGIN
        CREATE INDEX [IX_EmployeeLoans_ExternalEmployeeId] ON [EmployeeLoans] ([ExternalEmployeeId]);
    END;

    -- -------------------------------------------------
    -- 4. Index on UserId (owner lookup)
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_EmployeeLoans_UserId'
        AND object_id = OBJECT_ID(N'[EmployeeLoans]')
    )
    BEGIN
        CREATE INDEX [IX_EmployeeLoans_UserId] ON [EmployeeLoans] ([UserId]);
    END;

    -- -------------------------------------------------
    -- 5. Record migration in __EFMigrationsHistory
    -- -------------------------------------------------
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907200128_AddEmployeeLoans', N'8.0.0');

    PRINT 'Migration 20260907200128_AddEmployeeLoans applied successfully';
    PRINT 'Summary:';
    PRINT '  + EmployeeLoans table (Id, Amount, Description, LoanDate, UserId, ExternalEmployeeId, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)';
    PRINT '  + CK_EmployeeLoan_User_XOR_External (exactly one owner FK must be set)';
    PRINT '  + FK_EmployeeLoans_Users_UserId';
    PRINT '  + FK_EmployeeLoans_ExternalEmployees_ExternalEmployeeId (cascade)';
    PRINT '  + IX_EmployeeLoans_LoanDate';
    PRINT '  + IX_EmployeeLoans_ExternalEmployeeId';
    PRINT '  + IX_EmployeeLoans_UserId';
END
ELSE
BEGIN
    PRINT 'Migration 20260907200128_AddEmployeeLoans already applied - skipping';
END;
