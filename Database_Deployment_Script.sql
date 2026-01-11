-- ============================================
-- Wefaaq Database Deployment Script
-- Target: Azure SQL Database (SQL Server)
-- Purpose: Client and Organization Management System
-- Description: Complete schema deployment with tables, relationships, indexes, and seed data
-- Version: 1.0
-- Date: 2026-01-03
-- ============================================
-- NOTE: Make sure you are connected to your target database before running this script
-- Azure SQL Database does not support USE statements
-- ============================================

-- ============================================
-- SECTION 1: DROP EXISTING OBJECTS (IF ANY)
-- ============================================
PRINT 'SECTION 1: Dropping existing objects...'
GO

-- Drop foreign key constraints first
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_OrganizationCars_Organizations')
    ALTER TABLE [OrganizationCars] DROP CONSTRAINT [FK_OrganizationCars_Organizations]
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_OrganizationWorkers_Organizations')
    ALTER TABLE [OrganizationWorkers] DROP CONSTRAINT [FK_OrganizationWorkers_Organizations]
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_OrganizationLicenses_Organizations')
    ALTER TABLE [OrganizationLicenses] DROP CONSTRAINT [FK_OrganizationLicenses_Organizations]
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_OrganizationRecords_Organizations')
    ALTER TABLE [OrganizationRecords] DROP CONSTRAINT [FK_OrganizationRecords_Organizations]
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Organizations_Clients')
    ALTER TABLE [Organizations] DROP CONSTRAINT [FK_Organizations_Clients]
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Organizations')
    ALTER TABLE [Users] DROP CONSTRAINT [FK_Users_Organizations]
GO

-- Drop tables in reverse dependency order
IF OBJECT_ID('dbo.OrganizationCars', 'U') IS NOT NULL
    DROP TABLE [dbo].[OrganizationCars]
GO

IF OBJECT_ID('dbo.OrganizationWorkers', 'U') IS NOT NULL
    DROP TABLE [dbo].[OrganizationWorkers]
GO

IF OBJECT_ID('dbo.OrganizationLicenses', 'U') IS NOT NULL
    DROP TABLE [dbo].[OrganizationLicenses]
GO

IF OBJECT_ID('dbo.OrganizationRecords', 'U') IS NOT NULL
    DROP TABLE [dbo].[OrganizationRecords]
GO

IF OBJECT_ID('dbo.Organizations', 'U') IS NOT NULL
    DROP TABLE [dbo].[Organizations]
GO

IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users]
GO

IF OBJECT_ID('dbo.Clients', 'U') IS NOT NULL
    DROP TABLE [dbo].[Clients]
GO

PRINT 'Existing objects dropped successfully.'
GO

-- ============================================
-- SECTION 2: CREATE TABLES (WITHOUT FOREIGN KEYS)
-- ============================================
PRINT 'SECTION 2: Creating tables...'
GO

-- ============================================
-- Table: Clients (العملاء)
-- Purpose: Store client information with financial tracking
-- ============================================
CREATE TABLE [dbo].[Clients] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Name] NVARCHAR(255) NOT NULL,
    [Email] NVARCHAR(255) NOT NULL,
    [PhoneNumber] NVARCHAR(20) NULL,
    -- Classification: 1=Mumayyaz (مميز), 2=Aadi (عادى), 3=Mahwari (محورى)
    [Classification] INT NOT NULL DEFAULT 2,
    -- Balance: Negative = Debtor (مدين), Positive = Creditor (دائن)
    [Balance] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    [ExternalWorkersCount] INT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),

    -- Constraints
    CONSTRAINT [CK_Clients_Classification] CHECK ([Classification] IN (1, 2, 3)),
    CONSTRAINT [CK_Clients_Balance] CHECK ([Balance] >= -999999999.99 AND [Balance] <= 999999999.99),
    CONSTRAINT [CK_Clients_ExternalWorkersCount] CHECK ([ExternalWorkersCount] >= 0)
)
GO

-- ============================================
-- Table: Organizations (المؤسسات)
-- Purpose: Store organization information linked to clients
-- ============================================
CREATE TABLE [dbo].[Organizations] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Name] NVARCHAR(255) NOT NULL,
    [CardExpiringSoon] BIT NOT NULL DEFAULT 0,
    [ClientId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE()
)
GO

-- ============================================
-- Table: Users (المستخدمين)
-- Purpose: Store user authentication and profile information
-- ============================================
CREATE TABLE [dbo].[Users] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FirebaseUid] NVARCHAR(128) NOT NULL,
    [Email] NVARCHAR(255) NOT NULL,
    [Name] NVARCHAR(255) NULL,
    [Role] NVARCHAR(50) NULL,
    [OrganizationId] UNIQUEIDENTIFIER NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [LastLoginAt] DATETIME2(7) NULL,
    [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),

    -- Unique constraints
    CONSTRAINT [UQ_Users_FirebaseUid] UNIQUE ([FirebaseUid]),
    CONSTRAINT [UQ_Users_Email] UNIQUE ([Email])
)
GO

-- ============================================
-- Table: OrganizationRecords (سجلات المؤسسة)
-- Purpose: Store organization registration records
-- ============================================
CREATE TABLE [dbo].[OrganizationRecords] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Number] NVARCHAR(100) NOT NULL,
    [ExpiryDate] DATETIME2(7) NOT NULL,
    [ImagePath] NVARCHAR(500) NULL,
    [OrganizationId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE()
)
GO

-- ============================================
-- Table: OrganizationLicenses (تراخيص المؤسسة)
-- Purpose: Store organization business licenses
-- ============================================
CREATE TABLE [dbo].[OrganizationLicenses] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Number] NVARCHAR(100) NOT NULL,
    [ExpiryDate] DATETIME2(7) NOT NULL,
    [ImagePath] NVARCHAR(500) NULL,
    [OrganizationId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE()
)
GO

-- ============================================
-- Table: OrganizationWorkers (عمال المؤسسة)
-- Purpose: Store organization employee residence permits
-- ============================================
CREATE TABLE [dbo].[OrganizationWorkers] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Name] NVARCHAR(255) NOT NULL,
    [ResidenceNumber] NVARCHAR(50) NOT NULL,
    [ResidenceImagePath] NVARCHAR(500) NULL,
    [ExpiryDate] DATETIME2(7) NOT NULL,
    [OrganizationId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE()
)
GO

-- ============================================
-- Table: OrganizationCars (سيارات المؤسسة)
-- Purpose: Store organization vehicles and operating cards
-- ============================================
CREATE TABLE [dbo].[OrganizationCars] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [PlateNumber] NVARCHAR(20) NOT NULL,
    [Color] NVARCHAR(50) NOT NULL,
    [SerialNumber] NVARCHAR(50) NOT NULL,
    [ImagePath] NVARCHAR(500) NULL,
    [OperatingCardExpiry] DATETIME2(7) NOT NULL,
    [OrganizationId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE()
)
GO

PRINT 'Tables created successfully.'
GO

-- ============================================
-- SECTION 3: ADD FOREIGN KEY CONSTRAINTS
-- ============================================
PRINT 'SECTION 3: Adding foreign key constraints...'
GO

-- Organizations -> Clients relationship
ALTER TABLE [dbo].[Organizations]
    ADD CONSTRAINT [FK_Organizations_Clients]
    FOREIGN KEY ([ClientId])
    REFERENCES [dbo].[Clients]([Id])
    ON DELETE CASCADE
GO

-- Users -> Organizations relationship (optional)
ALTER TABLE [dbo].[Users]
    ADD CONSTRAINT [FK_Users_Organizations]
    FOREIGN KEY ([OrganizationId])
    REFERENCES [dbo].[Organizations]([Id])
    ON DELETE SET NULL
GO

-- OrganizationRecords -> Organizations relationship
ALTER TABLE [dbo].[OrganizationRecords]
    ADD CONSTRAINT [FK_OrganizationRecords_Organizations]
    FOREIGN KEY ([OrganizationId])
    REFERENCES [dbo].[Organizations]([Id])
    ON DELETE CASCADE
GO

-- OrganizationLicenses -> Organizations relationship
ALTER TABLE [dbo].[OrganizationLicenses]
    ADD CONSTRAINT [FK_OrganizationLicenses_Organizations]
    FOREIGN KEY ([OrganizationId])
    REFERENCES [dbo].[Organizations]([Id])
    ON DELETE CASCADE
GO

-- OrganizationWorkers -> Organizations relationship
ALTER TABLE [dbo].[OrganizationWorkers]
    ADD CONSTRAINT [FK_OrganizationWorkers_Organizations]
    FOREIGN KEY ([OrganizationId])
    REFERENCES [dbo].[Organizations]([Id])
    ON DELETE CASCADE
GO

-- OrganizationCars -> Organizations relationship
ALTER TABLE [dbo].[OrganizationCars]
    ADD CONSTRAINT [FK_OrganizationCars_Organizations]
    FOREIGN KEY ([OrganizationId])
    REFERENCES [dbo].[Organizations]([Id])
    ON DELETE CASCADE
GO

PRINT 'Foreign key constraints added successfully.'
GO

-- ============================================
-- SECTION 4: CREATE INDEXES FOR PERFORMANCE
-- ============================================
PRINT 'SECTION 4: Creating indexes...'
GO

-- Clients indexes
CREATE NONCLUSTERED INDEX [IX_Clients_Email] ON [dbo].[Clients]([Email])
GO

CREATE NONCLUSTERED INDEX [IX_Clients_Classification] ON [dbo].[Clients]([Classification])
GO

CREATE NONCLUSTERED INDEX [IX_Clients_CreatedAt] ON [dbo].[Clients]([CreatedAt] DESC)
GO

-- Organizations indexes
CREATE NONCLUSTERED INDEX [IX_Organizations_ClientId] ON [dbo].[Organizations]([ClientId])
GO

CREATE NONCLUSTERED INDEX [IX_Organizations_Name] ON [dbo].[Organizations]([Name])
GO

CREATE NONCLUSTERED INDEX [IX_Organizations_CardExpiringSoon] ON [dbo].[Organizations]([CardExpiringSoon])
GO

-- Users indexes
CREATE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]([Email])
GO

CREATE NONCLUSTERED INDEX [IX_Users_FirebaseUid] ON [dbo].[Users]([FirebaseUid])
GO

CREATE NONCLUSTERED INDEX [IX_Users_OrganizationId] ON [dbo].[Users]([OrganizationId])
GO

CREATE NONCLUSTERED INDEX [IX_Users_IsActive] ON [dbo].[Users]([IsActive])
GO

-- OrganizationRecords indexes
CREATE NONCLUSTERED INDEX [IX_OrganizationRecords_OrganizationId] ON [dbo].[OrganizationRecords]([OrganizationId])
GO

CREATE NONCLUSTERED INDEX [IX_OrganizationRecords_ExpiryDate] ON [dbo].[OrganizationRecords]([ExpiryDate])
GO

-- OrganizationLicenses indexes
CREATE NONCLUSTERED INDEX [IX_OrganizationLicenses_OrganizationId] ON [dbo].[OrganizationLicenses]([OrganizationId])
GO

CREATE NONCLUSTERED INDEX [IX_OrganizationLicenses_ExpiryDate] ON [dbo].[OrganizationLicenses]([ExpiryDate])
GO

-- OrganizationWorkers indexes
CREATE NONCLUSTERED INDEX [IX_OrganizationWorkers_OrganizationId] ON [dbo].[OrganizationWorkers]([OrganizationId])
GO

CREATE NONCLUSTERED INDEX [IX_OrganizationWorkers_ExpiryDate] ON [dbo].[OrganizationWorkers]([ExpiryDate])
GO

CREATE NONCLUSTERED INDEX [IX_OrganizationWorkers_ResidenceNumber] ON [dbo].[OrganizationWorkers]([ResidenceNumber])
GO

-- OrganizationCars indexes
CREATE NONCLUSTERED INDEX [IX_OrganizationCars_OrganizationId] ON [dbo].[OrganizationCars]([OrganizationId])
GO

CREATE NONCLUSTERED INDEX [IX_OrganizationCars_OperatingCardExpiry] ON [dbo].[OrganizationCars]([OperatingCardExpiry])
GO

CREATE NONCLUSTERED INDEX [IX_OrganizationCars_PlateNumber] ON [dbo].[OrganizationCars]([PlateNumber])
GO

PRINT 'Indexes created successfully.'
GO

-- ============================================
-- SECTION 5: INSERT SEED DATA
-- ============================================
PRINT 'SECTION 5: Inserting seed data...'
GO

-- Declare variables for storing IDs
DECLARE @AdminUserId INT
DECLARE @Client1Id UNIQUEIDENTIFIER = NEWID()
DECLARE @Client2Id UNIQUEIDENTIFIER = NEWID()
DECLARE @Client3Id UNIQUEIDENTIFIER = NEWID()
DECLARE @Client4Id UNIQUEIDENTIFIER = NEWID()
DECLARE @Client5Id UNIQUEIDENTIFIER = NEWID()
DECLARE @Org1Id UNIQUEIDENTIFIER = NEWID()
DECLARE @Org2Id UNIQUEIDENTIFIER = NEWID()
DECLARE @Org3Id UNIQUEIDENTIFIER = NEWID()
DECLARE @Org4Id UNIQUEIDENTIFIER = NEWID()
DECLARE @Org5Id UNIQUEIDENTIFIER = NEWID()

-- ============================================
-- Seed Users
-- ============================================
PRINT 'Inserting Users...'

INSERT INTO [dbo].[Users] ([FirebaseUid], [Email], [Name], [Role], [IsActive], [CreatedAt], [UpdatedAt])
VALUES
    ('3dMt71HRn7gBLiTYpHgy9UtKyWX2', 'admin@wefaaq.com', 'Administrator', 'Administrator', 1, GETUTCDATE(), GETUTCDATE()),
    ('newadmin_firebase_uid_placeholder', 'newadmin@wefaaq.com', 'New Admin', 'Administrator', 1, GETUTCDATE(), GETUTCDATE())

-- Get the inserted admin user ID
SET @AdminUserId = SCOPE_IDENTITY()

PRINT 'Users inserted: 2 admin users'

-- ============================================
-- Seed Clients
-- ============================================
PRINT 'Inserting Clients...'

-- Using pre-generated GUIDs
INSERT INTO [dbo].[Clients] ([Id], [Name], [Email], [PhoneNumber], [Classification], [Balance], [ExternalWorkersCount], [CreatedAt], [UpdatedAt])
VALUES
    (@Client1Id, 'Acme Corporation', 'contact@acme.com', '+1234567890', 1, 15000.50, 25, GETUTCDATE(), GETUTCDATE()),
    (@Client2Id, 'Tech Solutions Ltd', 'info@techsolutions.com', '+9876543210', 2, 8500.00, 12, GETUTCDATE(), GETUTCDATE()),
    (@Client3Id, 'Global Industries', 'support@globalind.com', '+1122334455', 3, 22000.75, 45, GETUTCDATE(), GETUTCDATE()),
    (@Client4Id, 'BuildRight Construction', 'admin@buildright.com', '+5544332211', 2, 5200.00, 8, GETUTCDATE(), GETUTCDATE()),
    (@Client5Id, 'Green Energy Co', 'contact@greenenergy.com', '+9988776655', 1, 18500.25, 32, GETUTCDATE(), GETUTCDATE())

PRINT 'Clients inserted: 5 records'

-- ============================================
-- Seed Organizations
-- ============================================
PRINT 'Inserting Organizations...'

INSERT INTO [dbo].[Organizations] ([Id], [Name], [CardExpiringSoon], [ClientId], [CreatedAt], [UpdatedAt])
VALUES
    (@Org1Id, 'Alpha Services', 0, @Client1Id, GETUTCDATE(), GETUTCDATE()),
    (@Org2Id, 'Beta Logistics', 1, @Client2Id, GETUTCDATE(), GETUTCDATE()),
    (@Org3Id, 'Gamma Manufacturing', 0, @Client1Id, GETUTCDATE(), GETUTCDATE()),
    (@Org4Id, 'Delta Transport', 1, @Client3Id, GETUTCDATE(), GETUTCDATE()),
    (@Org5Id, 'Epsilon Consulting', 0, @Client4Id, GETUTCDATE(), GETUTCDATE())

PRINT 'Organizations inserted: 5 records'

-- ============================================
-- Seed Organization Licenses
-- ============================================
PRINT 'Inserting Organization Licenses...'

INSERT INTO [dbo].[OrganizationLicenses] ([Id], [Number], [ExpiryDate], [OrganizationId], [CreatedAt], [UpdatedAt])
VALUES
    (NEWID(), 'LIC-2024-001', DATEADD(MONTH, 6, GETUTCDATE()), @Org1Id, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'LIC-2024-002', DATEADD(MONTH, 3, GETUTCDATE()), @Org2Id, GETUTCDATE(), GETUTCDATE())

PRINT 'Organization Licenses inserted: 2 records'

-- ============================================
-- Seed Organization Workers
-- ============================================
PRINT 'Inserting Organization Workers...'

INSERT INTO [dbo].[OrganizationWorkers] ([Id], [Name], [ResidenceNumber], [ExpiryDate], [OrganizationId], [CreatedAt], [UpdatedAt])
VALUES
    (NEWID(), 'John Smith', 'RES-001', DATEADD(YEAR, 1, GETUTCDATE()), @Org1Id, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Maria Garcia', 'RES-002', DATEADD(MONTH, 8, GETUTCDATE()), @Org1Id, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Ahmed Hassan', 'RES-003', DATEADD(MONTH, 5, GETUTCDATE()), @Org2Id, GETUTCDATE(), GETUTCDATE())

PRINT 'Organization Workers inserted: 3 records'

-- ============================================
-- Seed Organization Cars
-- ============================================
PRINT 'Inserting Organization Cars...'

INSERT INTO [dbo].[OrganizationCars] ([Id], [PlateNumber], [Color], [SerialNumber], [OperatingCardExpiry], [OrganizationId], [CreatedAt], [UpdatedAt])
VALUES
    (NEWID(), 'ABC-1234', 'White', 'SN-2024-001', DATEADD(MONTH, 10, GETUTCDATE()), @Org1Id, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'XYZ-5678', 'Blue', 'SN-2024-002', DATEADD(MONTH, 4, GETUTCDATE()), @Org2Id, GETUTCDATE(), GETUTCDATE())

PRINT 'Organization Cars inserted: 2 records'

PRINT 'Seed data insertion completed successfully.'
GO

-- ============================================
-- SECTION 6: VERIFICATION QUERIES
-- ============================================
PRINT 'SECTION 6: Running verification queries...'
GO

PRINT '=========================================='
PRINT 'DATABASE DEPLOYMENT VERIFICATION'
PRINT '=========================================='
PRINT ''

-- Verify Tables
PRINT 'Tables Created:'
SELECT
    TABLE_NAME AS [Table Name],
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS [Column Count]
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME
GO

PRINT ''
PRINT 'Foreign Keys Created:'
SELECT
    fk.name AS [Foreign Key Name],
    OBJECT_NAME(fk.parent_object_id) AS [From Table],
    OBJECT_NAME(fk.referenced_object_id) AS [To Table]
FROM sys.foreign_keys fk
ORDER BY OBJECT_NAME(fk.parent_object_id)
GO

PRINT ''
PRINT 'Indexes Created:'
SELECT
    OBJECT_NAME(i.object_id) AS [Table Name],
    i.name AS [Index Name],
    i.type_desc AS [Index Type]
FROM sys.indexes i
WHERE i.name IS NOT NULL
    AND OBJECTPROPERTY(i.object_id, 'IsUserTable') = 1
ORDER BY OBJECT_NAME(i.object_id), i.name
GO

PRINT ''
PRINT 'Record Counts:'
SELECT 'Users' AS [Table], COUNT(*) AS [Record Count] FROM [dbo].[Users]
UNION ALL
SELECT 'Clients', COUNT(*) FROM [dbo].[Clients]
UNION ALL
SELECT 'Organizations', COUNT(*) FROM [dbo].[Organizations]
UNION ALL
SELECT 'OrganizationRecords', COUNT(*) FROM [dbo].[OrganizationRecords]
UNION ALL
SELECT 'OrganizationLicenses', COUNT(*) FROM [dbo].[OrganizationLicenses]
UNION ALL
SELECT 'OrganizationWorkers', COUNT(*) FROM [dbo].[OrganizationWorkers]
UNION ALL
SELECT 'OrganizationCars', COUNT(*) FROM [dbo].[OrganizationCars]
GO

PRINT ''
PRINT 'Sample Data - Clients:'
SELECT TOP 3
    Id,
    Name,
    Email,
    CASE Classification
        WHEN 1 THEN 'Mumayyaz (مميز)'
        WHEN 2 THEN 'Aadi (عادى)'
        WHEN 3 THEN 'Mahwari (محورى)'
    END AS Classification,
    Balance,
    ExternalWorkersCount
FROM [dbo].[Clients]
ORDER BY CreatedAt
GO

PRINT ''
PRINT 'Sample Data - Organizations:'
SELECT TOP 3
    o.Id,
    o.Name AS [Organization Name],
    c.Name AS [Client Name],
    o.CardExpiringSoon
FROM [dbo].[Organizations] o
INNER JOIN [dbo].[Clients] c ON o.ClientId = c.Id
ORDER BY o.CreatedAt
GO

PRINT ''
PRINT 'Sample Data - Users:'
SELECT
    Id,
    Email,
    Name,
    Role,
    IsActive,
    CreatedAt
FROM [dbo].[Users]
GO

PRINT ''
PRINT '=========================================='
PRINT 'DATABASE DEPLOYMENT COMPLETED SUCCESSFULLY!'
PRINT '=========================================='
PRINT ''
PRINT 'Summary:'
PRINT '- 7 tables created'
PRINT '- 6 foreign key relationships established'
PRINT '- 20+ indexes created for performance'
PRINT '- Seed data inserted (5 clients, 5 organizations, 2 admin users, sample licenses, workers, and cars)'
PRINT ''
PRINT 'Next Steps:'
PRINT '1. Verify all tables are created correctly'
PRINT '2. Check foreign key relationships'
PRINT '3. Test your .NET application connection'
PRINT '4. Run your Entity Framework migrations if needed'
PRINT ''
PRINT 'Admin User Credentials (for Firebase):'
PRINT ''
PRINT '  Admin User 1:'
PRINT '    Email: admin@wefaaq.com'
PRINT '    Firebase UID: 3dMt71HRn7gBLiTYpHgy9UtKyWX2'
PRINT '    (Set password in Firebase Console)'
PRINT ''
PRINT '  Admin User 2 (New Admin):'
PRINT '    Email: newadmin@wefaaq.com'
PRINT '    Firebase UID: newadmin_firebase_uid_placeholder'
PRINT '    (Create user in Firebase Console and update UID)'
PRINT ''
GO
