namespace Book.Services
{
    public static class BookServiceExtensions
    {
        public static IServiceCollection AddBookServices(this IServiceCollection services)
        {
            /*
             * Although AddScoped is used as Book is entirely client side they are actually Singletons
             */

            services.AddScoped<IInitialiseSvc, InitialiseSvc>();
            services.AddScoped<IBookDbMigratorSvc, BookDbMigratorSvc>();
            services.AddScoped<IBookSettingRepository, BookSettingRepository>();
            services.AddScoped<IBookSettingSvc, BookSettingSvc>();
            services.AddScoped<ISummaryTypeRepository, SummaryTypeRepository>();
            services.AddScoped<ITransactionTypeRepository, TransactionTypeRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<PageParamsSvc>();
            services.AddSingleton<INotifierSvc, NotifierSvc>();

            return services;
        }
    }
}
