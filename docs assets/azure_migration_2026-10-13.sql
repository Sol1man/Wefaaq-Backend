-- =====================================================
-- Azure SQL Database Migration: Add Rents tables (الإيجارات)
-- Migration ID: 20261013095731_AddRents
-- Date: 2026-10-13
-- AZURE COMPATIBLE VERSION (No GO statements)
-- Description: Adds the Rents feature — assets the company rents out to its clients.
--              1. Rents        — the agreement: asset name (free text), renter,
--                                agreed monthly amount, start date, active flag.
--                                The renter is targeted the same four ways as a
--                                client operation (Client / Branch / Organization /
--                                external person held as free text).
--              2. RentCharges  — one row per rent per month holding that month's due,
--                                so a change to the agreed rent never rewrites history.
--                                Unique per (RentId, Year, Month) among live rows.
--              3. RentPayments — money received against a rent (دفعة), in full or in
--                                parts, attributed to the month of PaymentDate.
--              Rents keep their own ledger — Client.Balance is NOT touched.
--              Soft-delete enabled (IsDeleted / DeletedAt) like the other tables.
-- Previous migration applied in production: 20260907200128_AddEmployeeLoans
-- =====================================================

-- Check if migration already applied
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013095731_AddRents'
)
BEGIN
    -- -------------------------------------------------
    -- 1. Create the Rents table
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.tables WHERE name = N'Rents' AND schema_id = SCHEMA_ID(N'dbo')
    )
    BEGIN
        CREATE TABLE [Rents] (
            [Id]                     UNIQUEIDENTIFIER NOT NULL,
            [AssetName]              NVARCHAR(255)    NOT NULL,
            [Notes]                  NVARCHAR(1000)   NULL,
            [MonthlyAmount]          DECIMAL(18,2)    NOT NULL,
            [TargetType]             INT              NOT NULL,
            [ClientId]               UNIQUEIDENTIFIER NULL,
            [ClientBranchId]         UNIQUEIDENTIFIER NULL,
            [OrganizationId]         UNIQUEIDENTIFIER NULL,
            [ExternalPersonName]     NVARCHAR(255)    NULL,
            [ExternalPersonIdNumber] NVARCHAR(50)     NULL,
            [StartDate]              DATETIME2        NOT NULL,
            [IsActive]               BIT              NOT NULL CONSTRAINT [DF_Rents_IsActive]  DEFAULT (1),
            [CreatedAt]              DATETIME2        NOT NULL CONSTRAINT [DF_Rents_CreatedAt] DEFAULT (GETUTCDATE()),
            [UpdatedAt]              DATETIME2        NOT NULL CONSTRAINT [DF_Rents_UpdatedAt] DEFAULT (GETUTCDATE()),
            [IsDeleted]              BIT              NOT NULL CONSTRAINT [DF_Rents_IsDeleted] DEFAULT (0),
            [DeletedAt]              DATETIME2        NULL,
            CONSTRAINT [PK_Rents] PRIMARY KEY ([Id]),
            -- NO ACTION on all three, mirroring ClientOperations: it avoids the
            -- Client -> ClientBranch -> Rents cascade cycle.
            CONSTRAINT [FK_Rents_Clients_ClientId]
                FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]),
            CONSTRAINT [FK_Rents_ClientBranches_ClientBranchId]
                FOREIGN KEY ([ClientBranchId]) REFERENCES [ClientBranches] ([Id]),
            CONSTRAINT [FK_Rents_Organizations_OrganizationId]
                FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id])
        );
    END;

    -- -------------------------------------------------
    -- 2. Create the RentCharges table (one month's due)
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.tables WHERE name = N'RentCharges' AND schema_id = SCHEMA_ID(N'dbo')
    )
    BEGIN
        CREATE TABLE [RentCharges] (
            [Id]        UNIQUEIDENTIFIER NOT NULL,
            [RentId]    UNIQUEIDENTIFIER NOT NULL,
            [Year]      INT              NOT NULL,
            [Month]     INT              NOT NULL,
            [Amount]    DECIMAL(18,2)    NOT NULL,
            [CreatedAt] DATETIME2        NOT NULL CONSTRAINT [DF_RentCharges_CreatedAt] DEFAULT (GETUTCDATE()),
            [UpdatedAt] DATETIME2        NOT NULL CONSTRAINT [DF_RentCharges_UpdatedAt] DEFAULT (GETUTCDATE()),
            [IsDeleted] BIT              NOT NULL CONSTRAINT [DF_RentCharges_IsDeleted] DEFAULT (0),
            [DeletedAt] DATETIME2        NULL,
            CONSTRAINT [PK_RentCharges] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_RentCharges_Rents_RentId]
                FOREIGN KEY ([RentId]) REFERENCES [Rents] ([Id]) ON DELETE CASCADE
        );
    END;

    -- -------------------------------------------------
    -- 3. Create the RentPayments table (دفعة)
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.tables WHERE name = N'RentPayments' AND schema_id = SCHEMA_ID(N'dbo')
    )
    BEGIN
        CREATE TABLE [RentPayments] (
            [Id]          UNIQUEIDENTIFIER NOT NULL,
            [RentId]      UNIQUEIDENTIFIER NOT NULL,
            [Amount]      DECIMAL(18,2)    NOT NULL,
            [Description] NVARCHAR(500)    NOT NULL,
            [PaymentDate] DATETIME2        NOT NULL,
            [CreatedAt]   DATETIME2        NOT NULL CONSTRAINT [DF_RentPayments_CreatedAt] DEFAULT (GETUTCDATE()),
            [UpdatedAt]   DATETIME2        NOT NULL CONSTRAINT [DF_RentPayments_UpdatedAt] DEFAULT (GETUTCDATE()),
            [IsDeleted]   BIT              NOT NULL CONSTRAINT [DF_RentPayments_IsDeleted] DEFAULT (0),
            [DeletedAt]   DATETIME2        NULL,
            CONSTRAINT [PK_RentPayments] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_RentPayments_Rents_RentId]
                FOREIGN KEY ([RentId]) REFERENCES [Rents] ([Id]) ON DELETE CASCADE
        );
    END;

    -- -------------------------------------------------
    -- 4. Rents indexes
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.indexes WHERE name = N'IX_Rents_ClientId' AND object_id = OBJECT_ID(N'[Rents]')
    )
    BEGIN
        CREATE INDEX [IX_Rents_ClientId] ON [Rents] ([ClientId]);
    END;

    IF NOT EXISTS (
        SELECT * FROM sys.indexes WHERE name = N'IX_Rents_ClientBranchId' AND object_id = OBJECT_ID(N'[Rents]')
    )
    BEGIN
        CREATE INDEX [IX_Rents_ClientBranchId] ON [Rents] ([ClientBranchId]);
    END;

    IF NOT EXISTS (
        SELECT * FROM sys.indexes WHERE name = N'IX_Rents_OrganizationId' AND object_id = OBJECT_ID(N'[Rents]')
    )
    BEGIN
        CREATE INDEX [IX_Rents_OrganizationId] ON [Rents] ([OrganizationId]);
    END;

    IF NOT EXISTS (
        SELECT * FROM sys.indexes WHERE name = N'IX_Rents_TargetType' AND object_id = OBJECT_ID(N'[Rents]')
    )
    BEGIN
        CREATE INDEX [IX_Rents_TargetType] ON [Rents] ([TargetType]);
    END;

    IF NOT EXISTS (
        SELECT * FROM sys.indexes WHERE name = N'IX_Rents_IsActive' AND object_id = OBJECT_ID(N'[Rents]')
    )
    BEGIN
        CREATE INDEX [IX_Rents_IsActive] ON [Rents] ([IsActive]);
    END;

    IF NOT EXISTS (
        SELECT * FROM sys.indexes WHERE name = N'IX_Rents_AssetName' AND object_id = OBJECT_ID(N'[Rents]')
    )
    BEGIN
        CREATE INDEX [IX_Rents_AssetName] ON [Rents] ([AssetName]);
    END;

    -- -------------------------------------------------
    -- 5. RentCharges indexes — one live due per rent per month
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_RentCharges_RentId_Year_Month' AND object_id = OBJECT_ID(N'[RentCharges]')
    )
    BEGIN
        CREATE UNIQUE INDEX [IX_RentCharges_RentId_Year_Month]
            ON [RentCharges] ([RentId], [Year], [Month])
            WHERE [IsDeleted] = 0;
    END;

    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_RentCharges_Year_Month' AND object_id = OBJECT_ID(N'[RentCharges]')
    )
    BEGIN
        CREATE INDEX [IX_RentCharges_Year_Month] ON [RentCharges] ([Year], [Month]);
    END;

    -- -------------------------------------------------
    -- 6. RentPayments indexes
    -- -------------------------------------------------
    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_RentPayments_RentId' AND object_id = OBJECT_ID(N'[RentPayments]')
    )
    BEGIN
        CREATE INDEX [IX_RentPayments_RentId] ON [RentPayments] ([RentId]);
    END;

    IF NOT EXISTS (
        SELECT * FROM sys.indexes
        WHERE name = N'IX_RentPayments_PaymentDate' AND object_id = OBJECT_ID(N'[RentPayments]')
    )
    BEGIN
        CREATE INDEX [IX_RentPayments_PaymentDate] ON [RentPayments] ([PaymentDate]);
    END;

    -- -------------------------------------------------
    -- 7. Record migration in __EFMigrationsHistory
    -- -------------------------------------------------
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261013095731_AddRents', N'8.0.0');

    PRINT 'Migration 20261013095731_AddRents applied successfully';
    PRINT 'Summary:';
    PRINT '  + Rents table (Id, AssetName, Notes, MonthlyAmount, TargetType, ClientId, ClientBranchId, OrganizationId, ExternalPersonName, ExternalPersonIdNumber, StartDate, IsActive, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)';
    PRINT '  + FK_Rents_Clients_ClientId / FK_Rents_ClientBranches_ClientBranchId / FK_Rents_Organizations_OrganizationId (all NO ACTION)';
    PRINT '  + IX_Rents_ClientId, IX_Rents_ClientBranchId, IX_Rents_OrganizationId, IX_Rents_TargetType, IX_Rents_IsActive, IX_Rents_AssetName';
    PRINT '  + RentCharges table (Id, RentId, Year, Month, Amount, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)';
    PRINT '  + FK_RentCharges_Rents_RentId (cascade)';
    PRINT '  + IX_RentCharges_RentId_Year_Month (unique, filtered on IsDeleted = 0), IX_RentCharges_Year_Month';
    PRINT '  + RentPayments table (Id, RentId, Amount, Description, PaymentDate, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)';
    PRINT '  + FK_RentPayments_Rents_RentId (cascade)';
    PRINT '  + IX_RentPayments_RentId, IX_RentPayments_PaymentDate';
END
ELSE
BEGIN
    PRINT 'Migration 20261013095731_AddRents already applied - skipping';
END;
