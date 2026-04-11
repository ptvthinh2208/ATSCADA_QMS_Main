using ATSCADA_Library.DTOs;
using ATSCADA_Library.Entities;
using AutoMapper;

namespace ATSCADA_Library.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Counter, CounterDto>().ReverseMap();
            CreateMap<Modbus, ModbusDto>()
                //.ForMember(dest => dest.CounterId, opt => opt.MapFrom(src => src.CounterId))
                .ReverseMap();
        }
    }
}
