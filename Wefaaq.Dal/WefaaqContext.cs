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
    /// Client-Organization join table
    /// </summary>
    public DbSet<ClientOrganization> ClientOrganizations { get; set; }

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

        // Configure Organization entity
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure Client-Organization many-to-many relationship
        modelBuilder.Entity<ClientOrganization>(entity =>
        {
            entity.HasKey(e => new { e.ClientId, e.OrganizationId });

            entity.HasOne(e => e.Client)
                .WithMany(e => e.ClientOrganizations)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Organization)
                .WithMany(e => e.ClientOrganizations)
                .HasForeignKey(e => e.OrganizationId)
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

        // Configure many-to-many relationship for Client-Organization
        modelBuilder.Entity<Client>()
            .HasMany(e => e.Organizations)
            .WithMany(e => e.Clients)
            .UsingEntity<ClientOrganization>();
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
        }
    }
}