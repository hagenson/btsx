using BtsxWeb.Services;

namespace BtsxWeb.Models
{
    /// <summary>
    /// Defines class maps.
    /// </summary>
    public class AutoMapperConfig : AutoMapper.Profile
    {
        /// <summary>
        /// Initialises the maps.
        /// </summary>
        public AutoMapperConfig()
        {
            CreateMap<MigrationJob, MigrationJobModel>()
                .ForMember(dst => dst.JobId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.SourceServer, opt => opt.MapFrom(src => src.Request.SourceCredentials!.Server))
                .ForMember(dst => dst.FailedItems, opt => opt.MapFrom(src => src.Statistics!.FailedItems))
                .ForMember(dst => dst.ProgressUpdates, opt => opt.MapFrom(src => src.Request.ProgressUpdates))
                .ForMember(dst => dst.SkippedItems, opt => opt.MapFrom(src => src.Statistics!.SkippedItems))
                .ForMember(dst => dst.SuccessfulItems, opt => opt.MapFrom(src => src.Statistics!.SuccessfulItems))
                .ForMember(dst => dst.TotalItems, opt => opt.MapFrom(src => src.Statistics!.TotalItems))
                ;
        }
    }
}