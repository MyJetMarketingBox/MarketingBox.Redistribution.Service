using AutoMapper;
using MarketingBox.Affiliate.Service.Domain.Models.Affiliates;

namespace MarketingBox.Redistribution.Service.MapperProfiles
{
    public class AffiliateMapperProfile : Profile
    {
        public AffiliateMapperProfile()
        {
            CreateMap<Affiliate.Service.Domain.Models.Affiliates.Affiliate, AffiliateMessage>()
                .ForMember(x => x.AffiliateId, x => x.MapFrom(z => z.Id))
                .ForMember(x => x.GeneralInfo, x => x.MapFrom(z => z));
            CreateMap<Affiliate.Service.Domain.Models.Affiliates.Affiliate, GeneralInfo>();
            
        }
    }
}