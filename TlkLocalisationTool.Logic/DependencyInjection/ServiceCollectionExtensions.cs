using Microsoft.Extensions.DependencyInjection;
using System.Text;
using TlkLocalisationTool.Logic.Services;
using TlkLocalisationTool.Logic.Services.Interfaces;

namespace TlkLocalisationTool.Logic.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddJsonServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IJsonReader, JsonReader>();
        serviceCollection.AddSingleton<IJsonWriter, JsonWriter>();
    }

    public static void AddServices(this IServiceCollection serviceCollection)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        serviceCollection.AddSingleton<ITlkReader, TlkReader>();
        serviceCollection.AddSingleton<ITlkWriter, TlkWriter>();
        serviceCollection.AddSingleton<IGffReader, GffReader>();
        serviceCollection.AddSingleton<ILookupService, LookupService>();
        serviceCollection.AddSingleton<ITdaReader, TdaReader>();
        serviceCollection.AddSingleton<ISsfReader, SsfReader>();
    }
}
