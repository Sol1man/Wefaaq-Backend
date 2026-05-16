-- =====================================================
-- Azure SQL Database Migration: User Payments Account Feature
-- Migration ID: 20260510153613_AddPaymentTypeAndAccountAmounts
-- Date: 2026-05-16
-- AZURE COMPATIBLE VERSION (No GO statements)
-- Description: Adds account-amount fields to Users and Type / RelatedPaymentId
--              fields to UserPayments so payments can be classified as
--              Payment (0), Profit (1), or Initial (2 — admin top-up).
-- Previous migration applied in production: 20260411151016_AddClientOperations
-- =====================================================

-- Check if migration already applied
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510153613_AddPaymentTypeAndAccountAmounts'
)
BEGIN
    -- -------------------------------------------------
    -- 1. Users: add account-amount columns
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.columns
        WHERE object_id = OBJECT_ID(N'[Users]')
        AND name = 'InitialAccountAmount'
    )
    BEGIN
        ALTER TABLE [Users]
            ADD [InitialAccountAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_Users_InitialAccountAmount] DEFAULT (0);
    END;

    IF NOT EXISTS (
        SELECT * FROM sys.columns
        WHERE object_id = OBJECT_ID(N'[Users]')
        AND name = 'CurrentAccountAmount'
    )
    BEGIN
        ALTER TABLE [Users]
            ADD [CurrentAccountAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_Users_CurrentAccountAmount] DEFAULT (0);
    END;

    -- -------------------------------------------------
    -- 2. UserPayments: add Type (0=Payment, 1=Profit, 2=Initial)
    --    Default 0 backfills every existing row as Payment so the old
    --    flat-history semantics is preserved exactly.
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.columns
        WHERE object_id = OBJECT_ID(N'[UserPayments]')
        AND name = 'Type'
    )
    BEGIN
        ALTER TABLE [UserPayments]
            ADD [Type] INT NOT NULL CONSTRAINT [DF_UserPayments_Type] DEFAULT (0);
    END;

    -- -------------------------------------------------
    -- 3. UserPayments: add RelatedPaymentId — nullable self-FK
    --    Used to link a Profit row back to the Payment row it was
    --    submitted alongside (same client operation).
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.columns
        WHERE object_id = OBJECT_ID(N'[UserPayments]')
        AND name = 'RelatedPaymentId'
    )
    BEGIN
        ALTER TABLE [UserPayments]
            ADD [RelatedPaymentId] UNIQUEIDENTIFIER NULL;
    END;

    -- -------------------------------------------------
    -- 4. Indexes
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_UserPayments_Type'
        AND object_id = OBJECT_ID(N'[UserPayments]')
    )
    BEGIN
        CREATE INDEX [IX_UserPayments_Type] ON [UserPayments] ([Type]);
    END;

    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_UserPayments_RelatedPaymentId'
        AND object_id = OBJECT_ID(N'[UserPayments]')
    )
    BEGIN
        CREATE INDEX [IX_UserPayments_RelatedPaymentId] ON [UserPayments] ([RelatedPaymentId]);
    END;

    -- -------------------------------------------------
    -- 5. Self-referencing foreign key
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.foreign_keys
        WHERE name = N'FK_UserPayments_UserPayments_RelatedPaymentId'
        AND parent_object_id = OBJECT_ID(N'[UserPayments]')
    )
    BEGIN
        ALTER TABLE [UserPayments]
            ADD CONSTRAINT [FK_UserPayments_UserPayments_RelatedPaymentId]
            FOREIGN KEY ([RelatedPaymentId]) REFERENCES [UserPayments] ([Id]);
    END;

    -- -------------------------------------------------
    -- 6. Record migration in __EFMigrationsHistory
    -- -------------------------------------------------
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260510153613_AddPaymentTypeAndAccountAmounts', N'8.0.0');

    PRINT 'Migration 20260510153613_AddPaymentTypeAndAccountAmounts applied successfully';
    PRINT 'Summary:';
    PRINT '  + Users.InitialAccountAmount  decimal(18,2) NOT NULL default 0';
    PRINT '  + Users.CurrentAccountAmount  decimal(18,2) NOT NULL default 0';
    PRINT '  + UserPayments.Type           int NOT NULL default 0 (existing rows = Payment)';
    PRINT '  + UserPayments.RelatedPaymentId  uniqueidentifier NULL';
    PRINT '  + IX_UserPayments_Type, IX_UserPayments_RelatedPaymentId';
    PRINT '  + FK_UserPayments_UserPayments_RelatedPaymentId (self-FK)';
END
ELSE
BEGIN
    PRINT 'Migration 20260510153613_AddPaymentTypeAndAccountAmounts already applied - skipping';
END;
