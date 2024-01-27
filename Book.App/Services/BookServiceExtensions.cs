using Book.Models;

namespace Book.Services
{
    public static class BookServiceExtensions
    {
        public static IServiceCollection AddBookServices(this IServiceCollection services)
        {
            services.AddSingleton<BookDbMigratorSvc>();
            services.AddSingleton<BookSettingRepository>();
            services.AddSingleton<SummaryTypeRepository>();
            services.AddSingleton<TransactionTypeRepository>();
            services.AddSingleton<TransactionRepository>();
            services.AddSingleton<BookSettingSvc>();
            services.AddSingleton<MessageSvc>();

            return services;
        }

    }
}
