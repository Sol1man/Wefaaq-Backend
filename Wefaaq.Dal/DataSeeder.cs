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

		// Seed Users only if none exist
        if (!context.Users.Any())
        {
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
					FirebaseUid = "3dMt71HRn7gBLiTYpHgy9UtKyWX2",
					Email = "admin@wefaaq.com",
                    RoleId = 1, // Admin role (seeded in migration)
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            context.Users.AddRange(users);
            context.SaveChanges();
		}

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

        // Seed Client with Branches, and Branches with Organizations and External Workers
        if (!context.ClientBranches.Any())
        {
            var clientsList = context.Clients.ToList();
            if (clientsList.Count < 1)
            {
                return; // Need at least one client
            }

            // Create a client with branches
            var mainClient = clientsList[0]; // Using "Acme Corporation"

            // Create Client Branches
            var branches = new List<ClientBranch>
            {
                new ClientBranch
                {
                    Id = Guid.NewGuid(),
                    Name = "Acme Riyadh Branch",
                    Email = "riyadh@acme.com",
                    PhoneNumber = "+966112345678",
                    Classification = ClientClassification.Mumayyaz,
                    Balance = 8500.00m,
                    ParentClientId = mainClient.Id,
                    BranchType = "Regional Office",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ClientBranch
                {
                    Id = Guid.NewGuid(),
                    Name = "Acme Jeddah Branch",
                    Email = "jeddah@acme.com",
                    PhoneNumber = "+966122345678",
                    Classification = ClientClassification.Aadi,
                    Balance = 5200.00m,
                    ParentClientId = mainClient.Id,
                    BranchType = "Regional Office",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.ClientBranches.AddRange(branches);
            context.SaveChanges();

            var riyadhBranch = branches[0];
            var jeddahBranch = branches[1];

            // Create Organizations for the Riyadh Branch
            var branchOrganizations = new List<Organization>
            {
                new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = "Riyadh Cleaning Services",
                    CardExpiringSoon = false,
                    ClientId = null, // Organization belongs to branch, not direct client
                    ClientBranchId = riyadhBranch.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = "Riyadh Security Solutions",
                    CardExpiringSoon = true,
                    ClientId = null,
                    ClientBranchId = riyadhBranch.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = "Jeddah Maintenance Co",
                    CardExpiringSoon = false,
                    ClientId = null,
                    ClientBranchId = jeddahBranch.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.Organizations.AddRange(branchOrganizations);
            context.SaveChanges();

            // Add Organization Usernames to branch organizations
            var orgUsernames = new List<OrganizationUsername>
            {
                new OrganizationUsername
                {
                    Id = Guid.NewGuid(),
                    SiteName = "Qiwa Portal",
                    Username = "riyadh_cleaning",
                    Password = "secure_password_123",
                    OrganizationId = branchOrganizations[0].Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new OrganizationUsername
                {
                    Id = Guid.NewGuid(),
                    SiteName = "Muqeem Platform",
                    Username = "riyadh_security",
                    Password = "secure_password_456",
                    OrganizationId = branchOrganizations[1].Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new OrganizationUsername
                {
                    Id = Guid.NewGuid(),
                    SiteName = "Balady Portal",
                    Username = "jeddah_maintenance",
                    Password = "secure_password_789",
                    OrganizationId = branchOrganizations[2].Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.OrganizationUsernames.AddRange(orgUsernames);
            context.SaveChanges();

            // Create External Workers for branches
            var externalWorkers = new List<ExternalWorker>
            {
                // Workers for Riyadh Branch
                new ExternalWorker
                {
                    Id = Guid.NewGuid(),
                    Name = "Ahmed Mohammed",
                    WorkerType = WorkerType.HouseholdWorker,
                    ResidenceNumber = "RES-RYD-001",
                    ResidenceImagePath = "/images/ahmed_residence.jpg",
                    ExpiryDate = DateTime.UtcNow.AddYears(1),
                    ClientId = null,
                    ClientBranchId = riyadhBranch.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ExternalWorker
                {
                    Id = Guid.NewGuid(),
                    Name = "Fatima Ali",
                    WorkerType = WorkerType.HomeCook,
                    ResidenceNumber = "RES-RYD-002",
                    ResidenceImagePath = "/images/fatima_residence.jpg",
                    ExpiryDate = DateTime.UtcNow.AddMonths(8),
                    ClientId = null,
                    ClientBranchId = riyadhBranch.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ExternalWorker
                {
                    Id = Guid.NewGuid(),
                    Name = "Khalid Hassan",
                    WorkerType = WorkerType.PrivateDriver,
                    ResidenceNumber = "RES-RYD-003",
                    ResidenceImagePath = "/images/khalid_residence.jpg",
                    ExpiryDate = DateTime.UtcNow.AddMonths(10),
                    ClientId = null,
                    ClientBranchId = riyadhBranch.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Workers for Jeddah Branch
                new ExternalWorker
                {
                    Id = Guid.NewGuid(),
                    Name = "Salem Abdullah",
                    WorkerType = WorkerType.Farmer,
                    ResidenceNumber = "RES-JED-001",
                    ResidenceImagePath = "/images/salem_residence.jpg",
                    ExpiryDate = DateTime.UtcNow.AddMonths(6),
                    ClientId = null,
                    ClientBranchId = jeddahBranch.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ExternalWorker
                {
                    Id = Guid.NewGuid(),
                    Name = "Nasser Ibrahim",
                    WorkerType = WorkerType.Shepherd,
                    ResidenceNumber = "RES-JED-002",
                    ResidenceImagePath = "/images/nasser_residence.jpg",
                    ExpiryDate = DateTime.UtcNow.AddMonths(9),
                    ClientId = null,
                    ClientBranchId = jeddahBranch.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Worker directly under main client (not branch)
                new ExternalWorker
                {
                    Id = Guid.NewGuid(),
                    Name = "Omar Yousef",
                    WorkerType = WorkerType.Beekeeper,
                    ResidenceNumber = "RES-MAIN-001",
                    ResidenceImagePath = "/images/omar_residence.jpg",
                    ExpiryDate = DateTime.UtcNow.AddYears(2),
                    ClientId = mainClient.Id,
                    ClientBranchId = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.ExternalWorkers.AddRange(externalWorkers);
            context.SaveChanges();
        }
    }
}
