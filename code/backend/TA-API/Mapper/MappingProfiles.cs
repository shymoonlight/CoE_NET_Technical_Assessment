using AutoMapper;

namespace TA_API.Mapper
{
    public class MappingProfiles
    {
        /// <summary>
        /// Configures object-object mapping profiles for the application using the specified mapper configuration
        /// expression.
        /// </summary>}
        /// <param name="config">The mapper configuration expression to which mapping profiles are added. Cannot be null.</param>
        public static void Configure(IMapperConfigurationExpression config)
        {
            // Mapping profile for User entity and its DTOs
            config.AddProfile<UserProfile>();
        }
    }
}
