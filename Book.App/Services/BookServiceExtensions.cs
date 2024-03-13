using Book.Models;

namespace Book.Services
{
    public static class BookServiceExtensions
    {
        public static IServiceCollection AddBookServices(this IServiceCollection services)
        {
            services.AddSingleton<BookDbMigratorSvc>();
            services.AddSingleton<IBookSettingRepository, BookSettingRepository>();
            services.AddSingleton<ISummaryTypeRepository, SummaryTypeRepository>();
            services.AddSingleton<ITransactionTypeRepository, TransactionTypeRepository>();
            services.AddSingleton<ITransactionRepository, TransactionRepository>();
            services.AddSingleton<BookSettingSvc>();
            services.AddSingleton<MessageSvc>();

            return services;
        }

    }
}
