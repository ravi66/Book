namespace Book.Services
{
    public static class BookServiceExtensions
    {
        public static IServiceCollection AddBookServices(this IServiceCollection services)
        {
            // Scoped
            services.AddScoped<IBookDbMigratorSvc, BookDbMigratorSvc>();
            services.AddScoped<IBookSettingRepository, BookSettingRepository>();
            services.AddScoped<IBookSettingSvc, BookSettingSvc>();
            services.AddScoped<ISummaryTypeRepository, SummaryTypeRepository>();
            services.AddScoped<ITransactionTypeRepository, TransactionTypeRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<TransListSvc>();

            // Singletons
            services.AddSingleton<INotifierSvc, NotifierSvc>();

            return services;
        }
    }
}
