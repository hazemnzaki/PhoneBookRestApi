using AutoMapper;
using PhoneBookRestApi.Data.Models;
using PhoneBookRestApi.Dtos;

namespace PhoneBookRestApi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PhoneBookEntry, PhoneBookEntryDto>();
            CreateMap<PhoneBookEntryDto, PhoneBookEntry>();
            CreateMap<CreatePhoneBookEntryDto, PhoneBookEntry>();
            CreateMap<UpdatePhoneBookEntryDto, PhoneBookEntry>();
        }
    }
}
