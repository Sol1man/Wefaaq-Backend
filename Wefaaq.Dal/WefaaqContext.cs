using Microsoft.EntityFrameworkCore;
using Wefaaq.Dal.Entities;

namespace Wefaaq.Dal;

/// <summary>
/// Wefaaq database context
/// </summary>
public class WefaaqContext : DbContext
{
    public WefaaqContext(DbContextOptions<WefaaqContext> options) : base(options)
    {
    }

    /// <summary>
    /// Clients table (العملاء)
    /// </summary>
    public DbSet<Client> Clients { get; set; }

    /// <summary>
    /// Organizations table (المؤسسات)
    /// </summary>
    public DbSet<Organization> Organizations { get; set; }

    /// <summary>
    /// Organization records table (سجلات المؤسسات)
    /// </summary>
    public DbSet<OrganizationRecord> OrganizationRecords { get; set; }

    /// <summary>
    /// Organization licenses table (تراخيص المؤسسات)
    /// </summary>
    public DbSet<OrganizationLicense> OrganizationLicenses { get; set; }

    /// <summary>
    /// Organization workers table (عمال المؤسسات)
    /// </summary>
    public DbSet<OrganizationWorker> OrganizationWorkers { get; set; }

    /// <summary>
    /// Organization cars table (سيارات المؤسسات)
    /// </summary>
    public DbSet<OrganizationCar> OrganizationCars { get; set; }

    /// <summary>
    /// Users table (for authentication)
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Roles table (for authorization)
    /// </summary>
    public DbSet<Role> Roles { get; set; }

    /// <summary>
    /// External workers table (عمال خارجيين)
    /// </summary>
    public DbSet<ExternalWorker> ExternalWorkers { get; set; }

    /// <summary>
    /// Organization usernames table (اسماء المستخدمين للمؤسسات)
    /// </summary>
    public DbSet<OrganizationUsername> OrganizationUsernames { get; set; }

    /// <summary>
    /// Client branches table (فروع العملاء)
    /// </summary>
    public DbSet<ClientBranch> ClientBranches { get; set; }

    /// <summary>
    /// User payments table (مدفوعات المستخدمين)
    /// </summary>
    public DbSet<UserPayment> UserPayments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Client entity
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Index for email uniqueness
            entity.HasIndex(e => e.Email).IsUnique();

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Organization entity with one-to-many relationship to Client
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // One-to-many relationship: Client has many Organizations (nullable)
            // Using NoAction to avoid cascade cycle: Client -> ClientBranch -> Organizations
            entity.HasOne(e => e.Client)
                .WithMany(e => e.Organizations)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            // One-to-many relationship: ClientBranch has many Organizations (nullable)
            entity.HasOne(e => e.ClientBranch)
                .WithMany(e => e.Organizations)
                .HasForeignKey(e => e.ClientBranchId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            // CHECK constraint: must have either ClientId OR ClientBranchId (not both, not neither)
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Organization_Client_XOR_Branch",
                "([ClientId] IS NOT NULL AND [ClientBranchId] IS NULL) OR ([ClientId] IS NULL AND [ClientBranchId] IS NOT NULL)"
            ));

            // Add index for ClientBranchId
            entity.HasIndex(e => e.ClientBranchId);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure OrganizationRecord entity
        modelBuilder.Entity<OrganizationRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Number).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ImagePath).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Organization)
                .WithMany(e => e.Records)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure OrganizationLicense entity
        modelBuilder.Entity<OrganizationLicense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Number).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ImagePath).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Organization)
                .WithMany(e => e.Licenses)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure OrganizationWorker entity
        modelBuilder.Entity<OrganizationWorker>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ResidenceNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ResidenceImagePath).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Organization)
                .WithMany(e => e.Workers)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure OrganizationCar entity
        modelBuilder.Entity<OrganizationCar>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PlateNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Color).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ImagePath).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Organization)
                .WithMany(e => e.Cars)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);

            // Index for Name (unique)
            entity.HasIndex(e => e.Name).IsUnique();

            // Seed default roles
            entity.HasData(
                new Role { Id = 1, Name = "Admin", Description = "Full system access" },
                new Role { Id = 2, Name = "User", Description = "Standard user access" }
            );
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.FirebaseUid).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Index for FirebaseUid (unique)
            entity.HasIndex(e => e.FirebaseUid).IsUnique();
            // Index for Email (unique)
            entity.HasIndex(e => e.Email).IsUnique();

            // Relationship to Role (required)
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Optional relationship to Organization
            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });

        // Configure ExternalWorker entity
        modelBuilder.Entity<ExternalWorker>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ResidenceNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ResidenceImagePath).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Relationship to Client (nullable)
            // Using NoAction to avoid cascade cycle: Client -> ClientBranch -> ExternalWorkers
            entity.HasOne(e => e.Client)
                .WithMany(e => e.ExternalWorkers)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            // Relationship to ClientBranch (nullable)
            entity.HasOne(e => e.ClientBranch)
                .WithMany(e => e.ExternalWorkers)
                .HasForeignKey(e => e.ClientBranchId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            // CHECK constraint: must have either ClientId OR ClientBranchId (not both, not neither)
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_ExternalWorker_Client_XOR_Branch",
                "([ClientId] IS NOT NULL AND [ClientBranchId] IS NULL) OR ([ClientId] IS NULL AND [ClientBranchId] IS NOT NULL)"
            ));

            // Indexes
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.ClientBranchId);
            entity.HasIndex(e => e.ResidenceNumber);
            entity.HasIndex(e => e.ExpiryDate);
            entity.HasIndex(e => e.WorkerType);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure OrganizationUsername entity
        modelBuilder.Entity<OrganizationUsername>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.SiteName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Password).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Organization)
                .WithMany(e => e.Usernames)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.SiteName);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure ClientBranch entity
        modelBuilder.Entity<ClientBranch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.BranchType).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Relationship to parent Client
            entity.HasOne(e => e.ParentClient)
                .WithMany(e => e.ClientBranches)
                .HasForeignKey(e => e.ParentClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.ParentClientId);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Classification);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure UserPayment entity
        modelBuilder.Entity<UserPayment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Relationship to User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Client client)
            {
                if (entry.State == EntityState.Added)
                    client.CreatedAt = DateTime.UtcNow;
                client.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Organization organization)
            {
                if (entry.State == EntityState.Added)
                    organization.CreatedAt = DateTime.UtcNow;
                organization.UpdatedAt = DateTime.UtcNow;
            }
            // Add similar logic for other entities that have timestamps
            else if (entry.Entity is OrganizationRecord record)
            {
                if (entry.State == EntityState.Added)
                    record.CreatedAt = DateTime.UtcNow;
                record.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is OrganizationLicense license)
            {
                if (entry.State == EntityState.Added)
                    license.CreatedAt = DateTime.UtcNow;
                license.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is OrganizationWorker worker)
            {
                if (entry.State == EntityState.Added)
                    worker.CreatedAt = DateTime.UtcNow;
                worker.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is OrganizationCar car)
            {
                if (entry.State == EntityState.Added)
                    car.CreatedAt = DateTime.UtcNow;
                car.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is User user)
            {
                if (entry.State == EntityState.Added)
                    user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is ExternalWorker externalWorker)
            {
                if (entry.State == EntityState.Added)
                    externalWorker.CreatedAt = DateTime.UtcNow;
                externalWorker.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is OrganizationUsername username)
            {
                if (entry.State == EntityState.Added)
                    username.CreatedAt = DateTime.UtcNow;
                username.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is ClientBranch branch)
            {
                if (entry.State == EntityState.Added)
                    branch.CreatedAt = DateTime.UtcNow;
                branch.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is UserPayment payment)
            {
                if (entry.State == EntityState.Added)
                    payment.CreatedAt = DateTime.UtcNow;
                payment.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}