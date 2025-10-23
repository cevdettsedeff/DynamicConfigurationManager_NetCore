// ConfigurationReader.Application/Mappings/MappingProfile.cs
using AutoMapper;
using ConfigurationReader.Application.DTOs;
using ConfigurationReader.Domain.Entities;
using ConfigurationReader.Domain.Enums;

namespace ConfigurationReader.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ConfigurationItem <-> ConfigurationItemDto
        CreateMap<ConfigurationItem, ConfigurationItemDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString())); // ✅ Enum to string

        CreateMap<CreateConfigurationDto, ConfigurationItem>()
            .ForMember(dest => dest.Type, opt => opt.Ignore()) // ✅ Factory method kullanacağız
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());

        CreateMap<UpdateConfigurationDto, ConfigurationItem>()
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.Ignore())
            .ForMember(dest => dest.ApplicationName, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());
    }
}