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
        });

        // Configure Organization entity with one-to-many relationship to Client
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // One-to-many relationship: Client has many Organizations
            entity.HasOne(e => e.Client)
                .WithMany(e => e.Organizations)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
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
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.FirebaseUid).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Index for FirebaseUid (unique)
            entity.HasIndex(e => e.FirebaseUid).IsUnique();
            // Index for Email (unique)
            entity.HasIndex(e => e.Email).IsUnique();

            // Optional relationship to Organization
            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
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
        }
    }
}