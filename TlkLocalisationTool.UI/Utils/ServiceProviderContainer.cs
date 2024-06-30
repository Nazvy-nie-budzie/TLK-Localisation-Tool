using Microsoft.Extensions.DependencyInjection;
using System;

namespace TlkLocalisationTool.UI.Utils;

internal static class ServiceProviderContainer
{
    private static IServiceProvider _serviceProvider;

    public static void TrySetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider ??= serviceProvider;
    }

    public static object GetRequiredService(Type serviceType) => _serviceProvider.GetRequiredService(serviceType);

    public static T GetRequiredService<T>() => _serviceProvider.GetRequiredService<T>();
}
