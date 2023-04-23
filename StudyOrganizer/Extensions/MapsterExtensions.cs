using Mapster;

namespace StudyOrganizer.Extensions;

public static class MapsterExtensions
{
    public static TDestination UpdateWithDictionary<TDestination>(
        this TDestination source,
        IDictionary<string, string> propertyNameValuePairs)
    {
        var mappingConfig = new TypeAdapterConfig()
            .NewConfig<IDictionary<string, string>, TDestination>()
            .UseDestinationValue(member => !propertyNameValuePairs.ContainsKey(member.Name));

        propertyNameValuePairs.Adapt(source, mappingConfig.Config);
        return source;
    }
}