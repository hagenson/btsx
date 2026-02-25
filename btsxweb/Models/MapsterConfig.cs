using BtsxWeb.Services;
using Mapster;

namespace BtsxWeb.Models
{
    /// <summary>
    /// Defines class maps.
    /// </summary>
    public static class MapsterConfig
    {
        /// <summary>
        /// Initialises the maps.
        /// </summary>
        public static void Configure()
        {
            TypeAdapterConfig<MigrationJob, MigrationJobModel>
                .NewConfig()
                .Map(dst => dst.JobId, src => src.Id)
                .Map(dst => dst.SourceServer, src => src.Request.SourceCredentials!.Server)
                .Map(dst => dst.FailedItems, src => src.Statistics!.FailedItems)
                .Map(dst => dst.ProgressUpdates, src => src.Request.ProgressUpdates)
                .Map(dst => dst.SkippedItems, src => src.Statistics!.SkippedItems)
                .Map(dst => dst.SuccessfulItems, src => src.Statistics!.SuccessfulItems)
                .Map(dst => dst.TotalItems, src => src.Statistics!.TotalItems);
        }
    }
}
