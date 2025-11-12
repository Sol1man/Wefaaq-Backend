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
            .ForMember(dest => dest.Organizations, opt => opt.MapFrom(src => src.Organizations));

        CreateMap<ClientCreateDto, Client>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Organizations, opt => opt.Ignore());

        CreateMap<ClientUpdateDto, Client>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Organizations, opt => opt.Ignore());

        // Organization mappings
        CreateMap<Organization, OrganizationDto>()
            .ForMember(dest => dest.Records, opt => opt.MapFrom(src => src.Records))
            .ForMember(dest => dest.Licenses, opt => opt.MapFrom(src => src.Licenses))
            .ForMember(dest => dest.Workers, opt => opt.MapFrom(src => src.Workers))
            .ForMember(dest => dest.Cars, opt => opt.MapFrom(src => src.Cars))
            .ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Client != null ? src.Client.Name : string.Empty));

        CreateMap<OrganizationCreateDto, Organization>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Records, opt => opt.Ignore())
            .ForMember(dest => dest.Licenses, opt => opt.Ignore())
            .ForMember(dest => dest.Workers, opt => opt.Ignore())
            .ForMember(dest => dest.Cars, opt => opt.Ignore())
            .ForMember(dest => dest.Client, opt => opt.Ignore());

        CreateMap<OrganizationUpdateDto, Organization>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Records, opt => opt.Ignore())
            .ForMember(dest => dest.Licenses, opt => opt.Ignore())
            .ForMember(dest => dest.Workers, opt => opt.Ignore())
            .ForMember(dest => dest.Cars, opt => opt.Ignore())
            .ForMember(dest => dest.Client, opt => opt.Ignore());

        // Organization Record mappings
        CreateMap<OrganizationRecord, OrganizationRecordDto>();
        CreateMap<OrganizationRecordCreateDto, OrganizationRecord>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<OrganizationRecordUpdateDto, OrganizationRecord>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Organization License mappings
        CreateMap<OrganizationLicense, OrganizationLicenseDto>();
        CreateMap<OrganizationLicenseCreateDto, OrganizationLicense>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<OrganizationLicenseUpdateDto, OrganizationLicense>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Organization Worker mappings
        CreateMap<OrganizationWorker, OrganizationWorkerDto>();
        CreateMap<OrganizationWorkerCreateDto, OrganizationWorker>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<OrganizationWorkerUpdateDto, OrganizationWorker>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Organization Car mappings
        CreateMap<OrganizationCar, OrganizationCarDto>();
        CreateMap<OrganizationCarCreateDto, OrganizationCar>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<OrganizationCarUpdateDto, OrganizationCar>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}