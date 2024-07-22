using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Book.Models
{
    internal class BookDbContext(DbContextOptions<BookDbContext> options, IStringLocalizer<Resources.BookResources> Localizer) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = -1, Order = 99, Name = Localizer["Unknown"] });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = -1, TransactionTypeId = -1, Name = Localizer["Unknown"] });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<SummaryType> SummaryTypes { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<BookSetting> BookSetting { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // SQLite does not support expressions of type 'DateTimeOffset' in ORDER BY clauses. Convert the values to a supported type:
            configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToBinaryConverter>();
            configurationBuilder.Properties<DateTimeOffset?>().HaveConversion<DateTimeOffsetToBinaryConverter>();
        }
    }
}