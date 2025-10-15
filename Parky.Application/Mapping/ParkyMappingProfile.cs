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
            CreateMap<BookingDto, Booking>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Lot, opt => opt.Ignore());

            CreateMap<Booking, BookingDto>();
        }
    }
}
