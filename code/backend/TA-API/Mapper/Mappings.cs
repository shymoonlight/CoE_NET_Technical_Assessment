using AutoMapper;

namespace TA_API.Mapper
{
    public class Mappings
    {
        public static void Configure(IMapperConfigurationExpression config)
        {
            MappingProfiles.Configure(config);
        }
    }
}
