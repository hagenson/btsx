using Btsx;
using BtsxWeb.Services;

namespace BtsxWeb.Models
{
    public class AutoMapperConfig: AutoMapper.Profile
    {
        public AutoMapperConfig()
        {
            
            CreateMap<MigrationJob, MigrationJobModel>()
                .ForMember(dst => dst.JobId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.SourceServer, opt => opt.MapFrom(src => src.Request.SourceCredentials!.Server))
                .ForMember(dst => dst.FailedMessages, opt => opt.MapFrom(src => src.Statistics!.FailedMessages))
                .ForMember(dst => dst.ProgressUpdates, opt => opt.MapFrom(src => src.Request.ProgressUpdates))
                .ForMember(dst => dst.SkippedMessages, opt => opt.MapFrom(src => src.Statistics!.SkippedMessages))
                .ForMember(dst => dst.SuccessfulMessages, opt => opt.MapFrom(src => src.Statistics!.SuccessfulMessages))
                .ForMember(dst => dst.TotalMessages, opt => opt.MapFrom(src => src.Statistics!.TotalMessages))
                ;
        }
    }
}
