using AutoMapper;
using Wefaaq.Bll.DTOs;
using Wefaaq.Dal.Entities;

namespace Wefaaq.Bll.Mappings;

/// <summary>
/// AutoMapper profile for entity to DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Client mappings
        CreateMap<Client, ClientDto>()
            .ForMember(dest => dest.Organizations, opt => opt.MapFrom(src => src.Organizations))
            .ForMember(dest => dest.ExternalWorkers, opt => opt.MapFrom(src => src.ExternalWorkers))
            .ForMember(dest => dest.ClientBranches, opt => opt.MapFrom(src => src.ClientBranches));

        CreateMap<ClientCreateDto, Client>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Organizations, opt => opt.Ignore())
            .ForMember(dest => dest.ExternalWorkers, opt => opt.Ignore())
            .ForMember(dest => dest.ClientBranches, opt => opt.Ignore());

        CreateMap<ClientUpdateDto, Client>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Organizations, opt => opt.Ignore())
            .ForMember(dest => dest.ExternalWorkers, opt => opt.Ignore())
            .ForMember(dest => dest.ClientBranches, opt => opt.Ignore());

        // Organization mappings
        CreateMap<Organization, OrganizationDto>()
            .ForMember(dest => dest.Records, opt => opt.MapFrom(src => src.Records))
            .ForMember(dest => dest.Licenses, opt => opt.MapFrom(src => src.Licenses))
            .ForMember(dest => dest.Workers, opt => opt.MapFrom(src => src.Workers))
            .ForMember(dest => dest.Cars, opt => opt.MapFrom(src => src.Cars))
            .ForMember(dest => dest.Usernames, opt => opt.MapFrom(src => src.Usernames))
            .ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Client != null ? src.Client.Name : null))
            .ForMember(dest => dest.ClientBranch, opt => opt.MapFrom(src => src.ClientBranch != null ? src.ClientBranch.Name : null));

        CreateMap<OrganizationCreateDto, Organization>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Records, opt => opt.Ignore())
            .ForMember(dest => dest.Licenses, opt => opt.Ignore())
            .ForMember(dest => dest.Workers, opt => opt.Ignore())
            .ForMember(dest => dest.Cars, opt => opt.Ignore())
            .ForMember(dest => dest.Usernames, opt => opt.Ignore())
            .ForMember(dest => dest.Client, opt => opt.Ignore())
            .ForMember(dest => dest.ClientBranch, opt => opt.Ignore());

        CreateMap<OrganizationUpdateDto, Organization>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Records, opt => opt.Ignore())
            .ForMember(dest => dest.Licenses, opt => opt.Ignore())
            .ForMember(dest => dest.Workers, opt => opt.Ignore())
            .ForMember(dest => dest.Cars, opt => opt.Ignore())
            .ForMember(dest => dest.Usernames, opt => opt.Ignore())
            .ForMember(dest => dest.Client, opt => opt.Ignore())
            .ForMember(dest => dest.ClientBranch, opt => opt.Ignore());

        // Organization Record mappings
        CreateMap<OrganizationRecord, OrganizationRecordDto>();
        CreateMap<OrganizationRecordCreateDto, OrganizationRecord>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
        CreateMap<OrganizationRecordUpdateDto, OrganizationRecord>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        // Organization License mappings
        CreateMap<OrganizationLicense, OrganizationLicenseDto>();
        CreateMap<OrganizationLicenseCreateDto, OrganizationLicense>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
        CreateMap<OrganizationLicenseUpdateDto, OrganizationLicense>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        // Organization Worker mappings
        CreateMap<OrganizationWorker, OrganizationWorkerDto>();
        CreateMap<OrganizationWorkerCreateDto, OrganizationWorker>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
        CreateMap<OrganizationWorkerUpdateDto, OrganizationWorker>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        // Organization Car mappings
        CreateMap<OrganizationCar, OrganizationCarDto>();
        CreateMap<OrganizationCarCreateDto, OrganizationCar>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
        CreateMap<OrganizationCarUpdateDto, OrganizationCar>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<UserDto, User>()
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // External Worker mappings
        CreateMap<ExternalWorker, ExternalWorkerDto>()
            .ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Client != null ? src.Client.Name : null))
            .ForMember(dest => dest.ClientBranch, opt => opt.MapFrom(src => src.ClientBranch != null ? src.ClientBranch.Name : null));

        CreateMap<ExternalWorkerCreateDto, ExternalWorker>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Client, opt => opt.Ignore())
            .ForMember(dest => dest.ClientBranch, opt => opt.Ignore());

        CreateMap<ExternalWorkerUpdateDto, ExternalWorker>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Client, opt => opt.Ignore())
            .ForMember(dest => dest.ClientBranch, opt => opt.Ignore());

        // Organization Username mappings
        CreateMap<OrganizationUsername, OrganizationUsernameDto>();
        CreateMap<OrganizationUsernameCreateDto, OrganizationUsername>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<OrganizationUsernameUpdateDto, OrganizationUsername>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        // Client Branch mappings
        CreateMap<ClientBranch, ClientBranchDto>()
            .ForMember(dest => dest.ParentClient, opt => opt.MapFrom(src => src.ParentClient != null ? src.ParentClient.Name : string.Empty))
            .ForMember(dest => dest.Organizations, opt => opt.MapFrom(src => src.Organizations))
            .ForMember(dest => dest.ExternalWorkers, opt => opt.MapFrom(src => src.ExternalWorkers));

        CreateMap<ClientBranchCreateDto, ClientBranch>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ParentClient, opt => opt.Ignore())
            .ForMember(dest => dest.Organizations, opt => opt.Ignore())
            .ForMember(dest => dest.ExternalWorkers, opt => opt.Ignore());

        CreateMap<ClientBranchUpdateDto, ClientBranch>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ParentClient, opt => opt.Ignore())
            .ForMember(dest => dest.Organizations, opt => opt.Ignore())
            .ForMember(dest => dest.ExternalWorkers, opt => opt.Ignore());
    }
}