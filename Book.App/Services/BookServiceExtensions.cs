using Book.Models;

namespace Book.Services
{
    public static class BookServiceExtensions
    {
        public static IServiceCollection AddBookServices(this IServiceCollection services)
        {
            services.AddSingleton<BookDbMigratorSvc>();
            services.AddSingleton<BookSettingSvc>();
            services.AddSingleton<MessageSvc>();

            return services;
        }

    }
}
