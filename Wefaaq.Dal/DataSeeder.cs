using Microsoft.EntityFrameworkCore;
using Wefaaq.Dal.Entities;

namespace Wefaaq.Dal;

/// <summary>
/// Seeds initial data for the database
/// </summary>
public static class DataSeeder
{
    public static void SeedData(WefaaqContext context)
    {
        // ⚠️ IMPORTANT: Do NOT clear existing data!
        // Only seed if database is empty to avoid data loss

        // Seed Clients only if none exist
        if (!context.Clients.Any())
        {
            var clients = new List<Client>
            {
                new Client
                {
                    Id = Guid.NewGuid(),
                    Name = "Acme Corporation",
                    Email = "contact@acme.com",
                    PhoneNumber = "+1234567890",
                    Classification = ClientClassification.Mumayyaz,
                    Balance = 15000.50m,
                    ExternalWorkersCount = 25,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Client
                {
                    Id = Guid.NewGuid(),
                    Name = "Tech Solutions Ltd",
                    Email = "info@techsolutions.com",
                    PhoneNumber = "+9876543210",
                    Classification = ClientClassification.Aadi,
                    Balance = 8500.00m,
                    ExternalWorkersCount = 12,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Client
                {
                    Id = Guid.NewGuid(),
                    Name = "Global Industries",
                    Email = "support@globalind.com",
                    PhoneNumber = "+1122334455",
                    Classification = ClientClassification.Mahwari,
                    Balance = 22000.75m,
                    ExternalWorkersCount = 45,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Client
                {
                    Id = Guid.NewGuid(),
                    Name = "BuildRight Construction",
                    Email = "admin@buildright.com",
                    PhoneNumber = "+5544332211",
                    Classification = ClientClassification.Aadi,
                    Balance = 5200.00m,
                    ExternalWorkersCount = 8,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Client
                {
                    Id = Guid.NewGuid(),
                    Name = "Green Energy Co",
                    Email = "contact@greenenergy.com",
                    PhoneNumber = "+9988776655",
                    Classification = ClientClassification.Mumayyaz,
                    Balance = 18500.25m,
                    ExternalWorkersCount = 32,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.Clients.AddRange(clients);
            context.SaveChanges();
        }

        // Seed Organizations (must be seeded after Clients due to foreign key)
        if (!context.Organizations.Any())
        {
            var clientsList = context.Clients.ToList();
            if (clientsList.Count < 5)
            {
                return; // Need clients first
            }

            var organizations = new List<Organization>
            {
                new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = "Alpha Services",
                    CardExpiringSoon = false,
                    ClientId = clientsList[0].Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = "Beta Logistics",
                    CardExpiringSoon = true,
                    ClientId = clientsList[1].Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = "Gamma Manufacturing",
                    CardExpiringSoon = false,
                    ClientId = clientsList[0].Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = "Delta Transport",
                    CardExpiringSoon = true,
                    ClientId = clientsList[2].Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = "Epsilon Consulting",
                    CardExpiringSoon = false,
                    ClientId = clientsList[3].Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.Organizations.AddRange(organizations);
            context.SaveChanges();

            // Seed Organization Licenses
            var org1 = organizations[0];
            var org2 = organizations[1];

            var licenses = new List<OrganizationLicense>
            {
                new OrganizationLicense
                {
                    Id = Guid.NewGuid(),
                    Number = "LIC-2024-001",
                    ExpiryDate = DateTime.UtcNow.AddMonths(6),
                    OrganizationId = org1.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new OrganizationLicense
                {
                    Id = Guid.NewGuid(),
                    Number = "LIC-2024-002",
                    ExpiryDate = DateTime.UtcNow.AddMonths(3),
                    OrganizationId = org2.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.OrganizationLicenses.AddRange(licenses);

            // Seed Organization Workers
            var workers = new List<OrganizationWorker>
            {
                new OrganizationWorker
                {
                    Id = Guid.NewGuid(),
                    Name = "John Smith",
                    ResidenceNumber = "RES-001",
                    ExpiryDate = DateTime.UtcNow.AddYears(1),
                    OrganizationId = org1.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new OrganizationWorker
                {
                    Id = Guid.NewGuid(),
                    Name = "Maria Garcia",
                    ResidenceNumber = "RES-002",
                    ExpiryDate = DateTime.UtcNow.AddMonths(8),
                    OrganizationId = org1.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new OrganizationWorker
                {
                    Id = Guid.NewGuid(),
                    Name = "Ahmed Hassan",
                    ResidenceNumber = "RES-003",
                    ExpiryDate = DateTime.UtcNow.AddMonths(5),
                    OrganizationId = org2.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.OrganizationWorkers.AddRange(workers);

            // Seed Organization Cars
            var cars = new List<OrganizationCar>
            {
                new OrganizationCar
                {
                    Id = Guid.NewGuid(),
                    PlateNumber = "ABC-1234",
                    Color = "White",
                    SerialNumber = "SN-2024-001",
                    OperatingCardExpiry = DateTime.UtcNow.AddMonths(10),
                    OrganizationId = org1.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new OrganizationCar
                {
                    Id = Guid.NewGuid(),
                    PlateNumber = "XYZ-5678",
                    Color = "Blue",
                    SerialNumber = "SN-2024-002",
                    OperatingCardExpiry = DateTime.UtcNow.AddMonths(4),
                    OrganizationId = org2.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.OrganizationCars.AddRange(cars);
            context.SaveChanges();
        }
    }
}
