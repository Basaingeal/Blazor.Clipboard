using Microsoft.Extensions.DependencyInjection;

namespace CurrieTechnologies.Blazor.Clipboard
{
    public static class ExtensionMethods
    {
        public static IServiceCollection AddClipboard(this IServiceCollection services)
        {
            return services.AddSingleton<ClipboardService>();
        }
    }
}
