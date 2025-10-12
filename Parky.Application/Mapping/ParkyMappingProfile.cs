using AutoMapper;
using Parky.Application.Dtos;
using Parky.Domain.Entities;

namespace Parky.Application.Mapping
{
    public class ParkyMappingProfile : Profile
    {
        public ParkyMappingProfile()
        {
            CreateMap<ParkingLotDto, ParkingLot>().ReverseMap();
        }
    }
}
