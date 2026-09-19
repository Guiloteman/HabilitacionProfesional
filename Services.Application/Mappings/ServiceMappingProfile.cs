using AutoMapper;
using Services.Application.DTOs;
using Services.Domain.Entities;

namespace Services.Application.Mappings;

public class ServiceMappingProfile : Profile
{
    public ServiceMappingProfile()
    {
        // Define el mapeo de la entidad de base de datos hacia el DTO
        CreateMap<ServiceItem, ServiceItemDto>();
    }
}