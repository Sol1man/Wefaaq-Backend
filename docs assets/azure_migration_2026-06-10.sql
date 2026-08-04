-- =====================================================
-- Azure SQL Database Migration: Client Operations — Kind + remove Status
-- Migration ID: 20260610211701_RemoveOperationStatusAddKind
-- Date: 2026-06-10
-- AZURE COMPATIBLE VERSION (No GO statements)
-- Description: Removes the operation Status concept (every operation is
--              completed on creation) and introduces a Kind discriminator
--              on ClientOperations:
--                  Kind = 1  Service  (a service performed — debit)
--                  Kind = 2  Payment  (cash the client paid — credit)
--              Also makes Type nullable, since payment records carry no
--              operation type.
-- Previous migration applied in production: 20260602103722_AddCustomTypeToClientOperation
-- =====================================================

-- Check if migration already applied
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610211701_RemoveOperationStatusAddKind'
)
BEGIN
    -- -------------------------------------------------
    -- 1. Drop the Status index (if present)
    -- -------------------------------------------------
    IF EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_ClientOperations_Status'
        AND object_id = OBJECT_ID(N'[ClientOperations]')
    )
    BEGIN
        DROP INDEX [IX_ClientOperations_Status] ON [ClientOperations];
    END;

    -- -------------------------------------------------
    -- 2. Drop any default constraint on Status, then the column
    -- -------------------------------------------------
    IF EXISTS (
        SELECT * FROM sys.columns
        WHERE object_id = OBJECT_ID(N'[ClientOperations]')
        AND name = 'Status'
    )
    BEGIN
        DECLARE @df_status NVARCHAR(128);
        SELECT @df_status = dc.name
        FROM sys.default_constraints dc
        JOIN sys.columns c ON c.default_object_id = dc.object_id
        WHERE c.object_id = OBJECT_ID(N'[ClientOperations]')
        AND c.name = 'Status';

        IF @df_status IS NOT NULL
            EXEC('ALTER TABLE [ClientOperations] DROP CONSTRAINT [' + @df_status + ']');

        ALTER TABLE [ClientOperations] DROP COLUMN [Status];
    END;

    -- -------------------------------------------------
    -- 3. Make Type nullable (payment records have no operation type)
    -- -------------------------------------------------
    IF EXISTS (
        SELECT * FROM sys.columns
        WHERE object_id = OBJECT_ID(N'[ClientOperations]')
        AND name = 'Type'
        AND is_nullable = 0
    )
    BEGIN
        ALTER TABLE [ClientOperations] ALTER COLUMN [Type] INT NULL;
    END;

    -- -------------------------------------------------
    -- 4. Add Kind (1 = Service, 2 = Payment).
    --    Default 1 backfills every existing row as a Service operation,
    --    preserving the old debit-only semantics exactly.
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.columns
        WHERE object_id = OBJECT_ID(N'[ClientOperations]')
        AND name = 'Kind'
    )
    BEGIN
        ALTER TABLE [ClientOperations]
            ADD [Kind] INT NOT NULL CONSTRAINT [DF_ClientOperations_Kind] DEFAULT (1);
    END;

    -- -------------------------------------------------
    -- 5. Index on Kind
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_ClientOperations_Kind'
        AND object_id = OBJECT_ID(N'[ClientOperations]')
    )
    BEGIN
        CREATE INDEX [IX_ClientOperations_Kind] ON [ClientOperations] ([Kind]);
    END;

    -- -------------------------------------------------
    -- 6. Record migration in __EFMigrationsHistory
    -- -------------------------------------------------
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260610211701_RemoveOperationStatusAddKind', N'8.0.0');

    PRINT 'Migration 20260610211701_RemoveOperationStatusAddKind applied successfully';
    PRINT 'Summary:';
    PRINT '  - ClientOperations.Status         dropped (+ IX_ClientOperations_Status)';
    PRINT '  ~ ClientOperations.Type           now nullable';
    PRINT '  + ClientOperations.Kind           int NOT NULL default 1 (existing rows = Service)';
    PRINT '  + IX_ClientOperations_Kind';
END
ELSE
BEGIN
    PRINT 'Migration 20260610211701_RemoveOperationStatusAddKind already applied - skipping';
END;
