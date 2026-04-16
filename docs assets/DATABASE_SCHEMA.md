# Wefaaq Database Schema

## Overview
The Wefaaq database consists of 7 tables supporting client management, organizations, and their related records, licenses, workers, and vehicles.

## Tables

### 1. Users
Authentication and user management table with Firebase integration.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY(1,1) | Auto-incrementing user ID |
| FirebaseUid | NVARCHAR(128) | UNIQUE, NOT NULL | Firebase authentication UID |
| Email | NVARCHAR(255) | NOT NULL | User email address |
| Name | NVARCHAR(100) | NOT NULL | User full name |
| Role | NVARCHAR(50) | NOT NULL | User role (Admin, User, etc.) |
| OrganizationId | UNIQUEIDENTIFIER | NULLABLE, FK → Organizations.Id | Associated organization (nullable) |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Account active status |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Account creation timestamp |
| LastLoginAt | DATETIME2 | NULLABLE | Last login timestamp |
| UpdatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Last update timestamp |

**Indexes:**
- `IX_Users_FirebaseUid` (UNIQUE) on FirebaseUid
- `IX_Users_Email` on Email
- `IX_Users_OrganizationId` on OrganizationId

---

### 2. Clients (العملاء)
Customer/client management with financial tracking.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY, DEFAULT NEWID() | Unique client identifier |
| Name | NVARCHAR(200) | NOT NULL | Client name |
| Email | NVARCHAR(255) | NULLABLE | Client email address |
| PhoneNumber | NVARCHAR(20) | NULLABLE | Client phone number |
| Classification | INT | NOT NULL | Client classification (1=Mumayyaz, 2=Aadi, 3=Mahwari) |
| Balance | DECIMAL(18,2) | NOT NULL, DEFAULT 0 | Financial balance (negative=debtor, positive=creditor) |
| ExternalWorkersCount | INT | NOT NULL, DEFAULT 0 | Number of external workers |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Record creation timestamp |
| UpdatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Last update timestamp |

**Indexes:**
- `IX_Clients_Name` on Name
- `IX_Clients_Classification` on Classification

**Classification Enum:**
- `1` = Mumayyaz (مميز)
- `2` = Aadi (عادي)
- `3` = Mahwari (محوري)

---

### 3. Organizations (المؤسسات)
Business organizations belonging to clients.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY, DEFAULT NEWID() | Unique organization identifier |
| Name | NVARCHAR(200) | NOT NULL | Organization name |
| CardExpiringSoon | BIT | NOT NULL, DEFAULT 0 | Flag for expiring licenses/cards |
| ClientId | UNIQUEIDENTIFIER | NOT NULL, FK → Clients.Id | Parent client reference |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Record creation timestamp |
| UpdatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Last update timestamp |

**Indexes:**
- `IX_Organizations_ClientId` on ClientId
- `IX_Organizations_Name` on Name

**Foreign Keys:**
- `FK_Organizations_Clients` → Clients.Id (ON DELETE CASCADE)

**Relationships:**
- One Client has many Organizations
- One Organization belongs to one Client

---

### 4. OrganizationRecords (سجلات المؤسسات)
Official registration records for organizations.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY, DEFAULT NEWID() | Unique record identifier |
| Number | NVARCHAR(100) | NOT NULL | Registration/record number |
| ExpiryDate | DATETIME2 | NULLABLE | Record expiration date |
| ImagePath | NVARCHAR(500) | NULLABLE | Path to scanned document image |
| OrganizationId | UNIQUEIDENTIFIER | NOT NULL, FK → Organizations.Id | Parent organization reference |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Record creation timestamp |
| UpdatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Last update timestamp |

**Indexes:**
- `IX_OrganizationRecords_OrganizationId` on OrganizationId
- `IX_OrganizationRecords_Number` on Number

**Foreign Keys:**
- `FK_OrganizationRecords_Organizations` → Organizations.Id (ON DELETE CASCADE)

---

### 5. OrganizationLicenses (تراخيص المؤسسات)
Business licenses and permits for organizations.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY, DEFAULT NEWID() | Unique license identifier |
| Number | NVARCHAR(100) | NOT NULL | License number |
| ExpiryDate | DATETIME2 | NULLABLE | License expiration date |
| ImagePath | NVARCHAR(500) | NULLABLE | Path to license document image |
| OrganizationId | UNIQUEIDENTIFIER | NOT NULL, FK → Organizations.Id | Parent organization reference |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Record creation timestamp |
| UpdatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Last update timestamp |

**Indexes:**
- `IX_OrganizationLicenses_OrganizationId` on OrganizationId
- `IX_OrganizationLicenses_Number` on Number
- `IX_OrganizationLicenses_ExpiryDate` on ExpiryDate

**Foreign Keys:**
- `FK_OrganizationLicenses_Organizations` → Organizations.Id (ON DELETE CASCADE)

---

### 6. OrganizationWorkers (عمال المؤسسات)
Employee/worker records with residence permit tracking.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY, DEFAULT NEWID() | Unique worker identifier |
| Name | NVARCHAR(200) | NOT NULL | Worker full name |
| ResidenceNumber | NVARCHAR(100) | NOT NULL | Residence permit number (رقم الإقامة) |
| ResidenceImagePath | NVARCHAR(500) | NULLABLE | Path to residence permit image |
| ExpiryDate | DATETIME2 | NULLABLE | Residence permit expiration date |
| OrganizationId | UNIQUEIDENTIFIER | NOT NULL, FK → Organizations.Id | Parent organization reference |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Record creation timestamp |
| UpdatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Last update timestamp |

**Indexes:**
- `IX_OrganizationWorkers_OrganizationId` on OrganizationId
- `IX_OrganizationWorkers_ResidenceNumber` on ResidenceNumber
- `IX_OrganizationWorkers_ExpiryDate` on ExpiryDate

**Foreign Keys:**
- `FK_OrganizationWorkers_Organizations` → Organizations.Id (ON DELETE CASCADE)

---

### 7. OrganizationCars (سيارات المؤسسات)
Company vehicle records with operating card tracking.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY, DEFAULT NEWID() | Unique car identifier |
| PlateNumber | NVARCHAR(50) | NOT NULL | Vehicle license plate number |
| Color | NVARCHAR(50) | NULLABLE | Vehicle color |
| SerialNumber | NVARCHAR(100) | NULLABLE | Vehicle serial/chassis number |
| ImagePath | NVARCHAR(500) | NULLABLE | Path to vehicle document image |
| OperatingCardExpiry | DATETIME2 | NULLABLE | Operating card expiration date |
| OrganizationId | UNIQUEIDENTIFIER | NOT NULL, FK → Organizations.Id | Parent organization reference |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Record creation timestamp |
| UpdatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Last update timestamp |

**Indexes:**
- `IX_OrganizationCars_OrganizationId` on OrganizationId
- `IX_OrganizationCars_PlateNumber` on PlateNumber
- `IX_OrganizationCars_OperatingCardExpiry` on OperatingCardExpiry

**Foreign Keys:**
- `FK_OrganizationCars_Organizations` → Organizations.Id (ON DELETE CASCADE)

---

## Entity Relationships

```
Clients (1) ────< Organizations (N)
                      │
                      ├──< OrganizationRecords (N)
                      ├──< OrganizationLicenses (N)
                      ├──< OrganizationWorkers (N)
                      └──< OrganizationCars (N)

Organizations (1) ────< Users (N) [nullable relationship]
```

**Relationship Details:**
- One **Client** can have many **Organizations**
- One **Organization** belongs to one **Client**
- One **Organization** can have many:
  - **OrganizationRecords** (سجلات)
  - **OrganizationLicenses** (تراخيص)
  - **OrganizationWorkers** (عمال)
  - **OrganizationCars** (سيارات)
- One **Organization** can have many **Users** (nullable, for organization-specific users)

**Cascade Delete Behavior:**
- Deleting a **Client** cascades to delete all its **Organizations**
- Deleting an **Organization** cascades to delete all its:
  - Records
  - Licenses
  - Workers
  - Cars

---

## Common Patterns

### Timestamps
All tables include:
- `CreatedAt` (DATETIME2, NOT NULL, DEFAULT GETUTCDATE())
- `UpdatedAt` (DATETIME2, NOT NULL, DEFAULT GETUTCDATE())

These are automatically managed by the `WefaaqContext.SaveChanges()` override.

### Primary Keys
- **Users**: Integer identity column (INT IDENTITY)
- **All other tables**: UNIQUEIDENTIFIER with NEWID() default

### Nullable Fields
Most reference data fields (Email, PhoneNumber, ImagePath, ExpiryDate, etc.) are nullable to allow partial data entry and progressive data completion.

---

## Database Configuration

**Database Server**: Azure SQL Database
**Connection String Source**: Environment variable `CONNECTION_STRING` (Railway) or `appsettings.json` (local)
**Migration Strategy**: Entity Framework Core Code-First
**Seeding**: Automatic in Development mode via `DataSeeder.SeedData()`

---

## Notes

1. **Firebase Integration**: The Users table integrates with Firebase Authentication via the `FirebaseUid` column
2. **Financial Tracking**: Client.Balance supports negative values (debtor) and positive values (creditor)
3. **Expiry Tracking**: Multiple expiry date fields across licenses, residence permits, and operating cards
4. **Image Storage**: Image paths are stored as strings; actual images stored separately (file system or blob storage)
5. **Localization**: Field names and enums support Arabic business terminology
6. **Cascading Deletes**: All child records are automatically deleted when parent is removed
