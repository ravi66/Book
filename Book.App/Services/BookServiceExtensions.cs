using Book.Models;

namespace Book.Services
{
    public static class BookServiceExtensions
    {
        public static IServiceCollection AddBookServices(this IServiceCollection services)
        {
            services.AddSingleton<BookSettingSvc>();

            return services;
        }

    }
}
