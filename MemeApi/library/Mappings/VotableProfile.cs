using System.Linq;
using AutoMapper;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;

namespace MemeApi.library.Mappings
{
    public class VotableProfile : Profile
    {
        public VotableProfile()
        {
            CreateMap<MemeVisual, RandomComponentDTO>()
                .ForMember(dest => dest.data, opt => opt.MapFrom(src => src.Filename))
                .ForMember(dest => dest.votes, opt => opt.MapFrom(src => src.Votes.Aggregate(0, (acc, item) => acc + (item.Upvote ? 1 : -1))));
             
            CreateMap<MemeText, RandomComponentDTO>()
                .ForMember(dest => dest.data, opt => opt.MapFrom(src => src.Text))
                .ForMember(dest => dest.votes, opt => opt.MapFrom(src => src.Votes.Aggregate(0, (acc, item) => acc + (item.Upvote ? 1 : -1))));
        }
    }
}
